using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;

namespace PayFlow.API.Services;

public class AsaasService : IAsaasService
{
    private static readonly string[] PaymentWebhookEvents =
    [
        "PAYMENT_CREATED",
        "PAYMENT_UPDATED",
        "PAYMENT_CONFIRMED",
        "PAYMENT_RECEIVED",
        "PAYMENT_OVERDUE",
        "PAYMENT_DELETED",
        "PAYMENT_REFUNDED",
        "PAYMENT_PARTIALLY_REFUNDED",
        "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED",
        "PAYMENT_CHARGEBACK_REQUESTED",
        "PAYMENT_CHARGEBACK_DISPUTE",
        "PAYMENT_AWAITING_CHARGEBACK_REVERSAL"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly AsaasOptions _options;

    public AsaasService(IOptions<AsaasOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<AsaasSubaccountResult> CreateSubaccountAsync(
        AsaasSubaccountRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();

        var payload = new Dictionary<string, object?>
        {
            ["name"] = request.Name,
            ["email"] = request.Email,
            ["cpfCnpj"] = OnlyDigits(request.CpfCnpj),
            ["mobilePhone"] = OnlyDigits(request.MobilePhone),
            ["incomeValue"] = request.IncomeValue,
            ["address"] = request.Address,
            ["addressNumber"] = request.AddressNumber,
            ["province"] = request.Province,
            ["postalCode"] = OnlyDigits(request.PostalCode),
            ["companyType"] = NormalizeUpper(request.CompanyType),
            ["phone"] = OnlyDigits(request.Phone),
            ["complement"] = request.Complement,
            ["site"] = request.Site
        };

        var webhooks = BuildSubaccountWebhooks();
        if (webhooks != null)
            payload["webhooks"] = webhooks;

        using var client = CreateClient(_options.ApiKey);
        using var response = await client.PostAsync(
            $"{BaseUrl}/accounts",
            ToJsonContent(payload),
            cancellationToken);

        var root = await ReadJsonResponseAsync(response, cancellationToken);
        var accountId = GetString(root, "id");
        var walletId = GetString(root, "walletId");
        var apiKey = GetString(root, "apiKey");

        if (string.IsNullOrWhiteSpace(accountId) ||
            string.IsNullOrWhiteSpace(walletId) ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ValidationException("Asaas não retornou id, walletId ou apiKey da subconta");
        }

        return new AsaasSubaccountResult(accountId, walletId, apiKey);
    }

    public async Task<string> CreateCustomerAsync(
        string apiKey,
        string name,
        string? cpfCnpj,
        string? mobilePhone,
        string? email,
        string? externalReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ValidationException("Chave API da conta Asaas não configurada");

        var payload = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["cpfCnpj"] = OnlyDigits(cpfCnpj),
            ["mobilePhone"] = OnlyDigits(mobilePhone),
            ["email"] = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            ["externalReference"] = externalReference,
            ["notificationDisabled"] = true
        };

        using var client = CreateClient(apiKey);
        using var response = await client.PostAsync(
            $"{BaseUrl}/customers",
            ToJsonContent(payload),
            cancellationToken);

        var root = await ReadJsonResponseAsync(response, cancellationToken);
        var id = GetString(root, "id");
        if (string.IsNullOrWhiteSpace(id))
            throw new ValidationException("Asaas não retornou o ID do cliente");

        return id;
    }

    public async Task<string> CreatePlatformCustomerAsync(
        string name,
        string? cpfCnpj,
        string? mobilePhone,
        string? email,
        string? externalReference,
        CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();
        return await CreateCustomerAsync(
            _options.ApiKey,
            name,
            cpfCnpj,
            mobilePhone,
            email,
            externalReference,
            cancellationToken);
    }

    public async Task<AsaasChargeResult> CreatePaymentChargeAsync(
        Payment payment,
        string asaasCustomerId,
        string apiKey,
        string billingType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ValidationException("Chave API da subconta Asaas não configurada");

        var normalizedBillingType = NormalizeBillingType(billingType);
        var payload = new Dictionary<string, object?>
        {
            ["customer"] = asaasCustomerId,
            ["billingType"] = normalizedBillingType,
            ["value"] = payment.Amount,
            ["dueDate"] = payment.DueDate.ToString("yyyy-MM-dd"),
            ["description"] = $"Cobrança PayFlow {payment.TxId}",
            ["externalReference"] = payment.TxId
        };

        using var client = CreateClient(apiKey);
        using var response = await client.PostAsync(
            $"{BaseUrl}/payments",
            ToJsonContent(payload),
            cancellationToken);

        var root = await ReadJsonResponseAsync(response, cancellationToken);
        var paymentId = GetRequiredString(root, "id", "Asaas não retornou o ID da cobrança");
        var status = GetString(root, "status") ?? "PENDING";
        var invoiceUrl = GetString(root, "invoiceUrl") ?? GetString(root, "bankSlipUrl");
        var feeAmount = GetDecimal(root, "billingTypeFee");
        var netAmount = GetDecimal(root, "netValue");

        string? pixCopyPaste = null;
        string? pixImage = null;
        string? pixSvg = null;
        DateTime? pixExpiresAt = null;

        if (normalizedBillingType is "PIX" or "UNDEFINED")
        {
            var pix = await GetPixQrCodeAsync(paymentId, apiKey, cancellationToken);
            pixCopyPaste = pix.Payload;
            pixImage = pix.EncodedImage;
            pixExpiresAt = pix.ExpirationDate;
        }

        return new AsaasChargeResult(
            paymentId,
            status,
            normalizedBillingType,
            invoiceUrl,
            pixCopyPaste,
            pixImage,
            pixSvg,
            pixExpiresAt,
            feeAmount,
            netAmount);
    }

    public async Task<AsaasPaymentStatusResult> GetPaymentStatusAsync(
        string asaasPaymentId,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ValidationException("Chave API da conta Asaas não configurada");

        using var client = CreateClient(apiKey);
        using var response = await client.GetAsync($"{BaseUrl}/payments/{asaasPaymentId}", cancellationToken);
        var root = await ReadJsonResponseAsync(response, cancellationToken);

        var id = GetRequiredString(root, "id", "Asaas não retornou o ID da cobrança");
        var status = GetString(root, "status") ?? "PENDING";
        var billingType = GetString(root, "billingType") ?? "";
        var invoiceUrl = GetString(root, "invoiceUrl") ?? GetString(root, "bankSlipUrl");
        var paymentDate = GetDate(root, "paymentDate") ??
                          GetDate(root, "clientPaymentDate") ??
                          GetDate(root, "confirmedDate");

        return new AsaasPaymentStatusResult(
            id,
            status,
            billingType,
            invoiceUrl,
            paymentDate,
            GetDecimal(root, "billingTypeFee"),
            GetDecimal(root, "netValue"));
    }

    public async Task<AsaasSubscriptionResult> CreatePremiumSubscriptionAsync(
        string asaasCustomerId,
        Guid premiumSubscriptionId,
        Guid userId,
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();

        var now = DateTime.UtcNow;
        var payload = new Dictionary<string, object?>
        {
            ["customer"] = asaasCustomerId,
            ["billingType"] = NormalizeBillingType(_options.PremiumBillingType),
            ["value"] = _options.PremiumMonthlyAmount,
            ["nextDueDate"] = now.ToString("yyyy-MM-dd"),
            ["cycle"] = string.IsNullOrWhiteSpace(_options.PremiumCycle) ? "MONTHLY" : _options.PremiumCycle.Trim().ToUpperInvariant(),
            ["description"] = "PayFlow Premium",
            ["externalReference"] = $"premium:{premiumSubscriptionId}",
            ["notificationDisabled"] = true
        };

        using var client = CreateClient(_options.ApiKey);
        using var response = await client.PostAsync(
            $"{BaseUrl}/subscriptions",
            ToJsonContent(payload),
            cancellationToken);

        var root = await ReadJsonResponseAsync(response, cancellationToken);
        var subscriptionId = GetRequiredString(root, "id", "Asaas não retornou o ID da assinatura");
        var status = GetString(root, "status") ?? "PENDING";
        var checkoutUrl = GetString(root, "invoiceUrl") ??
                          await GetFirstSubscriptionPaymentUrlAsync(client, subscriptionId, cancellationToken);

        return new AsaasSubscriptionResult(
            subscriptionId,
            status,
            asaasCustomerId,
            checkoutUrl,
            now,
            now.AddMonths(1));
    }

    public async Task CancelSubscriptionAsync(
        string asaasSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();

        using var client = CreateClient(_options.ApiKey);
        using var response = await client.DeleteAsync($"{BaseUrl}/subscriptions/{asaasSubscriptionId}", cancellationToken);
        await ReadJsonResponseAsync(response, cancellationToken);
    }

    private async Task<AsaasPixQrCodeResult> GetPixQrCodeAsync(
        string asaasPaymentId,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var client = CreateClient(apiKey);
        using var response = await client.GetAsync($"{BaseUrl}/payments/{asaasPaymentId}/pixQrCode", cancellationToken);
        var root = await ReadJsonResponseAsync(response, cancellationToken);

        var image = GetString(root, "encodedImage");
        if (!string.IsNullOrWhiteSpace(image) && !image.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            image = $"data:image/png;base64,{image}";

        return new AsaasPixQrCodeResult(
            GetString(root, "payload"),
            image,
            GetDate(root, "expirationDate"));
    }

    private async Task<string?> GetFirstSubscriptionPaymentUrlAsync(
        HttpClient client,
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"{BaseUrl}/subscriptions/{subscriptionId}/payments?limit=1", cancellationToken);
        var root = await ReadJsonResponseAsync(response, cancellationToken);

        if (!root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0)
        {
            return null;
        }

        var firstPayment = data[0];
        return GetString(firstPayment, "invoiceUrl") ?? GetString(firstPayment, "bankSlipUrl");
    }

    private HttpClient CreateClient(string apiKey)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            string.IsNullOrWhiteSpace(_options.UserAgent) ? "PayFlow/1.0" : _options.UserAgent);
        client.DefaultRequestHeaders.TryAddWithoutValidation("access_token", apiKey);
        return client;
    }

    private string BaseUrl => string.IsNullOrWhiteSpace(_options.BaseUrl)
        ? "https://api-sandbox.asaas.com/v3"
        : _options.BaseUrl.TrimEnd('/');

    private IReadOnlyList<Dictionary<string, object?>>? BuildSubaccountWebhooks()
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookUrl))
            return null;

        if (string.IsNullOrWhiteSpace(_options.WebhookEmail))
            throw new ValidationException("Asaas:WebhookEmail precisa ser configurado para criar webhooks de subcontas");

        ValidateWebhookAuthToken();

        return
        [
            new Dictionary<string, object?>
            {
                ["name"] = "PayFlow pagamentos",
                ["url"] = _options.WebhookUrl.Trim(),
                ["email"] = _options.WebhookEmail.Trim(),
                ["enabled"] = true,
                ["interrupted"] = false,
                ["apiVersion"] = 3,
                ["authToken"] = _options.WebhookAuthToken.Trim(),
                ["sendType"] = "SEQUENTIALLY",
                ["events"] = PaymentWebhookEvents
            }
        ];
    }

    private void ValidateWebhookAuthToken()
    {
        var token = _options.WebhookAuthToken?.Trim();
        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Asaas:WebhookAuthToken precisa ser configurado para criar webhooks de subcontas");

        if (token.Length < 32 || token.Length > 255 || token.Any(char.IsWhiteSpace))
        {
            throw new ValidationException(
                "Asaas:WebhookAuthToken deve ter entre 32 e 255 caracteres e não pode conter espaços");
        }
    }

    private void EnsureRootConfigured()
    {
        if (!IsConfigured)
            throw new ValidationException("Configure Asaas:ApiKey para usar a integração Asaas");
    }

    private static StringContent ToJsonContent(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    private static async Task<JsonElement> ReadJsonResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new ValidationException(BuildAsaasErrorMessage(body, response));

        if (string.IsNullOrWhiteSpace(body))
            return JsonDocument.Parse("{}").RootElement.Clone();

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new ValidationException($"Resposta inválida do Asaas: {ex.Message}");
        }
    }

    private static string BuildAsaasErrorMessage(string body, HttpResponseMessage response)
    {
        if (string.IsNullOrWhiteSpace(body))
            return $"Asaas retornou HTTP {(int)response.StatusCode}";

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("errors", out var errors) &&
                errors.ValueKind == JsonValueKind.Array &&
                errors.GetArrayLength() > 0)
            {
                var descriptions = errors.EnumerateArray()
                    .Select(error => GetString(error, "description"))
                    .Where(description => !string.IsNullOrWhiteSpace(description));

                var message = string.Join("; ", descriptions);
                if (!string.IsNullOrWhiteSpace(message))
                    return $"Asaas: {message}";
            }
        }
        catch
        {
            // Return raw body below.
        }

        return $"Asaas retornou HTTP {(int)response.StatusCode}: {body}";
    }

    private static string GetRequiredString(JsonElement element, string propertyName, string errorMessage)
    {
        return GetString(element, propertyName) ?? throw new ValidationException(errorMessage);
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

    private static string NormalizeBillingType(string billingType)
    {
        var normalized = billingType.Trim().ToUpperInvariant();
        return normalized switch
        {
            "PIX" => "PIX",
            "CARD" => "CREDIT_CARD",
            "CREDIT_CARD" => "CREDIT_CARD",
            "CARTAO" => "CREDIT_CARD",
            "CARTÃO" => "CREDIT_CARD",
            "UNDEFINED" => "UNDEFINED",
            _ => throw new ValidationException("Forma de pagamento Asaas inválida. Use PIX, CREDIT_CARD ou UNDEFINED.")
        };
    }

    private static string? OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private static string? NormalizeUpper(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    private record AsaasPixQrCodeResult(string? Payload, string? EncodedImage, DateTime? ExpirationDate);
}
