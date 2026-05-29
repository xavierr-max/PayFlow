using Microsoft.Extensions.Options;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;
using Stripe;
using Stripe.Checkout;
using CustomerEntity = PayFlow.Domain.Sales.Entities.Customer;

namespace PayFlow.API.Services;

public interface IStripeCheckoutService
{
    Task<StripeCheckoutResult> CreateCardCheckoutSessionAsync(
        Payment payment,
        CustomerEntity customer,
        string storeName,
        string? connectedAccountId,
        CancellationToken cancellationToken);
}

public record StripeCheckoutResult(
    string SessionId,
    string CheckoutUrl,
    DateTime? ExpiresAt,
    string? PaymentIntentId);

public class StripeCheckoutService : IStripeCheckoutService
{
    private readonly StripeOptions _options;

    public StripeCheckoutService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StripeCheckoutResult> CreateCardCheckoutSessionAsync(
        Payment payment,
        CustomerEntity customer,
        string storeName,
        string? connectedAccountId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new ValidationException("Configure Stripe:SecretKey para gerar cobranças por cartão");

        try
        {
            var metadata = new Dictionary<string, string>
            {
                ["payflow_payment_id"] = payment.Id.ToString(),
                ["payflow_customer_id"] = customer.Id.ToString(),
                ["payflow_txid"] = payment.TxId,
                ["payflow_payment_method"] = "card",
                ["payflow_store_name"] = storeName,
                ["payflow_installment"] = $"{payment.InstallmentNumber}/{payment.TotalInstallments}"
            };

            var service = new SessionService(new StripeClient(_options.SecretKey));

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = _options.CheckoutSuccessUrl,
                CancelUrl = _options.CheckoutCancelUrl,
                ClientReferenceId = payment.Id.ToString(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = metadata,
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = metadata
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "brl",
                            UnitAmount = ToCents(payment.Amount),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = storeName,
                                Description = $"Parcela {payment.InstallmentNumber}/{payment.TotalInstallments} | Cliente: {customer.Name} | TXID: {payment.TxId}"
                            }
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(connectedAccountId))
            {
                sessionOptions.PaymentIntentData.TransferData = new SessionPaymentIntentDataTransferDataOptions
                {
                    Destination = connectedAccountId
                };
            }

            var session = await service.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(session.Url))
                throw new ValidationException("Stripe não retornou a URL do Checkout");

            return new StripeCheckoutResult(session.Id, session.Url, session.ExpiresAt, session.PaymentIntentId);
        }
        catch (StripeException ex)
        {
            var message = ex.StripeError?.Message ?? ex.Message;
            throw new ValidationException($"Stripe cartão: {message}");
        }
    }

    private static long ToCents(decimal value)
    {
        return checked((long)Math.Round(value * 100, 0, MidpointRounding.AwayFromZero));
    }
}
