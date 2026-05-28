using Microsoft.Extensions.Options;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;
using Stripe;
using CustomerEntity = PayFlow.Domain.Sales.Entities.Customer;

namespace PayFlow.API.Services;

public interface IStripePixService
{
    Task<StripePixResult> CreatePixAsync(Payment payment, CustomerEntity customer, string storeName, CancellationToken cancellationToken);
}

public record StripePixResult(
    string PaymentIntentId,
    string PaymentStatus,
    string CopyPaste,
    string? QrCodeImageUrl,
    string? QrCodeSvgUrl,
    string? HostedInstructionsUrl,
    DateTime? ExpiresAt);

public class StripePixService : IStripePixService
{
    private readonly StripeOptions _options;

    public StripePixService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StripePixResult> CreatePixAsync(Payment payment, CustomerEntity customer, string storeName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new ValidationException("Configure Stripe:SecretKey para gerar cobranças PIX");

        try
        {
            var service = new PaymentIntentService(new StripeClient(_options.SecretKey));
            var intent = await service.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToCents(payment.Amount),
                Currency = "brl",
                Confirm = true,
                Description = $"{storeName} - cobrança {payment.InstallmentNumber}/{payment.TotalInstallments}",
                PaymentMethodTypes = new List<string> { "pix" },
                PaymentMethodData = new PaymentIntentPaymentMethodDataOptions
                {
                    Type = "pix",
                    BillingDetails = new PaymentIntentPaymentMethodDataBillingDetailsOptions
                    {
                        Name = customer.Name,
                        Phone = NormalizePhone(customer.Phone)
                    }
                },
                PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
                {
                    Pix = new PaymentIntentPaymentMethodOptionsPixOptions
                    {
                        ExpiresAfterSeconds = _options.PixExpiresAfterSeconds
                    }
                },
                Metadata = new Dictionary<string, string>
                {
                    ["payflow_payment_id"] = payment.Id.ToString(),
                    ["payflow_customer_id"] = customer.Id.ToString(),
                    ["payflow_txid"] = payment.TxId,
                    ["payflow_store_name"] = storeName,
                    ["payflow_installment"] = $"{payment.InstallmentNumber}/{payment.TotalInstallments}"
                }
            }, cancellationToken: cancellationToken);

            var pix = intent.NextAction?.PixDisplayQrCode;
            if (pix == null || string.IsNullOrWhiteSpace(pix.Data))
                throw new ValidationException("Stripe não retornou os dados do QR Code PIX");

            return new StripePixResult(
                intent.Id,
                intent.Status,
                pix.Data,
                pix.ImageUrlPng,
                pix.ImageUrlSvg,
                pix.HostedInstructionsUrl,
                pix.ExpiresAt);
        }
        catch (StripeException ex)
        {
            var message = ex.StripeError?.Message ?? ex.Message;

            if (message.Contains("payment method type \"pix\" is invalid", StringComparison.OrdinalIgnoreCase))
            {
                message = "Pix não está habilitado para esta conta Stripe. Ative Pix em Dashboard > Settings > Payment methods, confirme que a conta é elegível para Pix/BRL e tente novamente.";
            }

            throw new ValidationException($"Stripe PIX: {message}");
        }
    }

    private static long ToCents(decimal value)
    {
        return checked((long)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero));
    }

    private static string? NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            return null;

        if (!digits.StartsWith("55") && (digits.Length == 10 || digits.Length == 11))
            digits = $"55{digits}";

        return $"+{digits}";
    }
}
