using Microsoft.Extensions.Options;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Billing.Entities;
using Stripe;
using Stripe.Checkout;

namespace PayFlow.API.Services;

public interface IStripePaymentStatusService
{
    Task<StripePaymentStatusResult> GetStatusAsync(Payment payment, CancellationToken cancellationToken);
}

public record StripePaymentStatusResult(
    string PaymentStatus,
    string? PaymentIntentId,
    string? CheckoutSessionId);

public class StripePaymentStatusService : IStripePaymentStatusService
{
    private readonly StripeOptions _options;

    public StripePaymentStatusService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StripePaymentStatusResult> GetStatusAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new ValidationException("Configure Stripe:SecretKey para atualizar status de pagamentos Stripe");

        try
        {
            var client = new StripeClient(_options.SecretKey);

            if (!string.IsNullOrWhiteSpace(payment.StripeCheckoutSessionId))
                return await GetCheckoutSessionStatusAsync(client, payment.StripeCheckoutSessionId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
                return await GetPaymentIntentStatusAsync(client, payment.StripePaymentIntentId, cancellationToken);

            throw new ValidationException("Esta cobrança ainda não tem um pagamento Stripe para sincronizar");
        }
        catch (StripeException ex)
        {
            var message = ex.StripeError?.Message ?? ex.Message;
            throw new ValidationException($"Stripe status: {message}");
        }
    }

    private static async Task<StripePaymentStatusResult> GetCheckoutSessionStatusAsync(
        StripeClient client,
        string checkoutSessionId,
        CancellationToken cancellationToken)
    {
        var sessionService = new SessionService(client);
        var session = await sessionService.GetAsync(checkoutSessionId, cancellationToken: cancellationToken);
        var paymentStatus = session.PaymentStatus ?? "";

        if (!paymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(session.PaymentIntentId))
        {
            var paymentIntentStatus = await GetPaymentIntentStatusAsync(client, session.PaymentIntentId, cancellationToken);
            paymentStatus = PreferCompletedStatus(paymentStatus, paymentIntentStatus.PaymentStatus);
        }

        return new StripePaymentStatusResult(
            paymentStatus,
            session.PaymentIntentId,
            session.Id);
    }

    private static async Task<StripePaymentStatusResult> GetPaymentIntentStatusAsync(
        StripeClient client,
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        var paymentIntentService = new PaymentIntentService(client);
        var intent = await paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

        return new StripePaymentStatusResult(
            intent.Status,
            intent.Id,
            null);
    }

    private static string PreferCompletedStatus(string currentStatus, string nextStatus)
    {
        if (nextStatus.Equals("succeeded", StringComparison.OrdinalIgnoreCase) ||
            nextStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
        {
            return nextStatus;
        }

        return string.IsNullOrWhiteSpace(currentStatus) ? nextStatus : currentStatus;
    }
}
