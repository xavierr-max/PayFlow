using System.Text.Json;
using Microsoft.Extensions.Options;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;
using PayFlow.Domain.Sales.Enum;

namespace PayFlow.API.Services;

public interface IAsaasWebhookService
{
    Task<WebhookProcessingResult> ProcessAsync(
        string rawPayload,
        string? receivedAuthToken,
        CancellationToken cancellationToken);
}

public record WebhookProcessingResult(
    bool Processed,
    string Message,
    Guid? PaymentId = null,
    Guid? SellerId = null);

public class AsaasWebhookService : IAsaasWebhookService
{
    private const string Provider = "asaas";

    private readonly IDataRepository _repository;
    private readonly IPremiumSubscriptionService _premiumSubscriptionService;
    private readonly INotificationService _notificationService;
    private readonly AsaasOptions _options;

    public AsaasWebhookService(
        IDataRepository repository,
        IPremiumSubscriptionService premiumSubscriptionService,
        INotificationService notificationService,
        IOptions<AsaasOptions> options)
    {
        _repository = repository;
        _premiumSubscriptionService = premiumSubscriptionService;
        _notificationService = notificationService;
        _options = options.Value;
    }

    public async Task<WebhookProcessingResult> ProcessAsync(
        string rawPayload,
        string? receivedAuthToken,
        CancellationToken cancellationToken)
    {
        ValidateWebhookToken(receivedAuthToken);

        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;
        var eventType = GetString(root, "event") ?? GetString(root, "type") ?? "asaas_event";
        var paymentNode = ExtractPaymentNode(root);

        var asaasPaymentId = paymentNode.HasValue ? GetString(paymentNode.Value, "id") : null;
        var status = paymentNode.HasValue ? GetString(paymentNode.Value, "status") ?? eventType : eventType;
        var eventId = GetString(root, "id") ??
                      $"{eventType}:{asaasPaymentId ?? "unknown"}:{status}:{GetString(root, "dateCreated") ?? ""}";

        var log = _repository.GetWebhookEventLog(Provider, eventId);
        if (log?.IsProcessed == true)
            return new WebhookProcessingResult(false, "Webhook já processado", log.PaymentId, log.SellerId);

        if (log == null)
        {
            log = new WebhookEventLog(Provider, eventId, eventType, rawPayload);
            _repository.AddWebhookEventLog(log);
        }

        try
        {
            var result = await ApplyPaymentEventAsync(
                eventType,
                eventId,
                paymentNode,
                asaasPaymentId,
                status,
                cancellationToken);

            log.MarkProcessed(result.PaymentId, result.SellerId);
            _repository.UpdateWebhookEventLog(log);
            return result;
        }
        catch (Exception ex)
        {
            log.MarkFailed(ex.Message);
            _repository.UpdateWebhookEventLog(log);
            throw;
        }
    }

    private async Task<WebhookProcessingResult> ApplyPaymentEventAsync(
        string eventType,
        string eventId,
        JsonElement? paymentNode,
        string? asaasPaymentId,
        string status,
        CancellationToken cancellationToken)
    {
        if (!paymentNode.HasValue)
            return new WebhookProcessingResult(true, "Evento Asaas sem objeto payment");

        var paymentElement = paymentNode.Value;
        var externalReference = GetString(paymentElement, "externalReference");
        var paidAt = GetDate(paymentElement, "paymentDate") ??
                     GetDate(paymentElement, "clientPaymentDate") ??
                     GetDate(paymentElement, "confirmedDate");
        var amount = GetDecimal(paymentElement, "value") ?? 0m;
        var currency = "brl";

        if (TryGetPremiumSubscriptionId(externalReference, out var premiumSubscriptionId))
        {
            return await _premiumSubscriptionService.ProcessAsaasPaymentAsync(
                premiumSubscriptionId,
                asaasPaymentId ?? eventId,
                status,
                amount,
                currency,
                eventId,
                eventType,
                paidAt,
                cancellationToken);
        }

        var payment = FindPayment(externalReference, asaasPaymentId);
        if (payment == null)
            return new WebhookProcessingResult(true, "Cobrança PayFlow não encontrada para evento Asaas");

        var previousStatus = payment.Status;
        payment.SyncAsaasPaymentStatus(status, paidAt);
        payment.AttachAsaasFinancialDetails(
            GetDecimal(paymentElement, "billingTypeFee"),
            GetDecimal(paymentElement, "netValue"));

        _repository.UpdatePayment(payment);
        _repository.AddPaymentTransaction(new PaymentTransaction(
            payment.Id,
            payment.SellerId,
            Provider,
            MapTransactionType(eventType, status),
            status,
            amount > 0 ? amount : payment.Amount,
            currency,
            paymentMethod: GetString(paymentElement, "billingType") ?? payment.AsaasBillingType,
            providerEventId: eventId,
            externalProviderPaymentId: asaasPaymentId,
            notes: eventType,
            occurredAt: paidAt));

        NotifyPaymentStatusChange(payment, previousStatus, eventType);
        return new WebhookProcessingResult(true, "Pagamento Asaas processado", payment.Id, payment.SellerId);
    }

    private Payment? FindPayment(string? externalReference, string? asaasPaymentId)
    {
        if (!string.IsNullOrWhiteSpace(externalReference))
        {
            var byReference = _repository.GetPaymentByTxId(externalReference);
            if (byReference != null)
                return byReference;
        }

        return string.IsNullOrWhiteSpace(asaasPaymentId)
            ? null
            : _repository.GetPaymentByAsaasPaymentId(asaasPaymentId);
    }

    private void NotifyPaymentStatusChange(Payment payment, PaymentStatus previousStatus, string eventType)
    {
        if (payment.Status == previousStatus)
            return;

        if (payment.Status == PaymentStatus.Paid)
        {
            _notificationService.NotifySeller(
                payment.SellerId,
                "payment_paid",
                "Pagamento recebido",
                $"A cobrança {payment.TxId} foi paga via Asaas.",
                targetUrl: $"/payments/{payment.Id}",
                deduplicationKey: $"payment_paid:{payment.Id}:{payment.PaidAt:O}");
            return;
        }

        if (payment.Status == PaymentStatus.Cancelled)
        {
            _notificationService.NotifySeller(
                payment.SellerId,
                "payment_cancelled",
                "Cobrança cancelada",
                $"A cobrança {payment.TxId} foi cancelada no Asaas.",
                targetUrl: $"/payments/{payment.Id}",
                deduplicationKey: $"payment_cancelled_provider:{payment.Id}:{eventType}");
        }
    }

    private void ValidateWebhookToken(string? receivedAuthToken)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookAuthToken))
            return;

        if (!string.Equals(_options.WebhookAuthToken, receivedAuthToken, StringComparison.Ordinal))
            throw new UnauthorizedException("Webhook Asaas com token inválido");
    }

    private static JsonElement? ExtractPaymentNode(JsonElement root)
    {
        if (root.TryGetProperty("payment", out var payment))
            return payment;

        if (root.TryGetProperty("data", out var data))
            return data;

        if (root.TryGetProperty("object", out var obj))
            return obj;

        return null;
    }

    private static bool TryGetPremiumSubscriptionId(string? externalReference, out Guid subscriptionId)
    {
        subscriptionId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(externalReference) ||
            !externalReference.StartsWith("premium:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Guid.TryParse(externalReference["premium:".Length..], out subscriptionId);
    }

    private static string MapTransactionType(string eventType, string status)
    {
        var normalized = status.ToUpperInvariant();
        if (normalized is "RECEIVED" or "CONFIRMED" or "RECEIVED_IN_CASH")
            return "payment_received";

        if (normalized is "CANCELED" or "CANCELLED")
            return "payment_cancelled";

        if (normalized is "OVERDUE")
            return "payment_overdue";

        if (normalized is "REFUNDED")
            return "refund";

        return eventType.ToLowerInvariant();
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            return null;

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var value))
            return value;

        return decimal.TryParse(property.ToString(), out var parsed) ? parsed : null;
    }

    private static DateTime? GetDate(JsonElement element, string propertyName)
    {
        var raw = GetString(element, propertyName);
        return DateTime.TryParse(raw, out var date) ? NormalizeDate(date) : null;
    }

    private static DateTime NormalizeDate(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }
}
