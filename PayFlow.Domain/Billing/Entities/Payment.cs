using PayFlow.Domain.Sales.Enum;
using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Represents a payment installment.
/// </summary>
public class Payment : Entity
{
    protected Payment() { }

    public Payment(DateTime dueDate, decimal amount, int number, int total, Guid customerId)
    {
        Validate(amount, number, total, customerId);

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        DueDate = dueDate;

        Amount = amount;
        InstallmentNumber = number;
        TotalInstallments = total;
        CustomerId = customerId;

        TxId = Guid.CreateVersion7().ToString("N");
    }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public decimal Amount { get; private set; }

    public int InstallmentNumber { get; private set; }
    public int TotalInstallments { get; private set; }

    public string TxId { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? StripePaymentStatus { get; private set; }
    public string? PixCopyPaste { get; private set; }
    public string? PixQrCodeImageUrl { get; private set; }
    public string? PixQrCodeSvgUrl { get; private set; }
    public string? PixHostedInstructionsUrl { get; private set; }
    public DateTime? PixExpiresAt { get; private set; }
    public DateTime? PixMessageSentAt { get; private set; }
    public string? StripePaymentMethod { get; private set; }
    public string? StripeCheckoutSessionId { get; private set; }
    public string? StripeCheckoutUrl { get; private set; }
    public DateTime? StripeCheckoutUrlExpiresAt { get; private set; }
    public DateTime? PaymentMessageSentAt { get; private set; }
    public DateTime? StockDeductedAt { get; private set; }

    public Guid CustomerId { get; private set; }

    public void MarkAsPaid(decimal valueReceived, DateTime? paidAt = null)
    {
        if (valueReceived != Amount)
            throw new InvalidOperationException("Invalid payment amount");

        Status = PaymentStatus.Paid;
        PaidAt = NormalizePaidAt(paidAt);
    }

    public void MarkAsOverdue()
    {
        if (Status == PaymentStatus.Pending && DateTime.UtcNow > DueDate)
        {
            Status = PaymentStatus.Overdue;
        }
    }

    public bool HasActivePix(DateTime utcNow)
    {
        return !string.IsNullOrWhiteSpace(PixCopyPaste) &&
               PixExpiresAt.HasValue &&
               PixExpiresAt.Value > utcNow;
    }

    public bool HasActiveCheckoutSession(DateTime utcNow, string paymentMethod)
    {
        return StripePaymentMethod?.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase) == true &&
               !string.IsNullOrWhiteSpace(StripeCheckoutUrl) &&
               StripeCheckoutUrlExpiresAt.HasValue &&
               StripeCheckoutUrlExpiresAt.Value > utcNow;
    }

    public void AttachStripePix(
        string paymentIntentId,
        string paymentStatus,
        string pixCopyPaste,
        string? pixQrCodeImageUrl,
        string? pixQrCodeSvgUrl,
        string? pixHostedInstructionsUrl,
        DateTime? pixExpiresAt)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("Invalid Stripe payment intent");

        if (string.IsNullOrWhiteSpace(pixCopyPaste))
            throw new ArgumentException("Invalid PIX code");

        StripePaymentIntentId = paymentIntentId;
        StripePaymentStatus = paymentStatus;
        PixCopyPaste = pixCopyPaste;
        PixQrCodeImageUrl = pixQrCodeImageUrl;
        PixQrCodeSvgUrl = pixQrCodeSvgUrl;
        PixHostedInstructionsUrl = pixHostedInstructionsUrl;
        PixExpiresAt = pixExpiresAt;
        StripePaymentMethod = "pix";
    }

    public void AttachStripeCheckoutSession(
        string checkoutSessionId,
        string checkoutUrl,
        DateTime? checkoutUrlExpiresAt,
        string paymentMethod,
        string? paymentIntentId)
    {
        if (string.IsNullOrWhiteSpace(checkoutSessionId))
            throw new ArgumentException("Invalid Stripe checkout session");

        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new ArgumentException("Invalid Stripe checkout URL");

        StripeCheckoutSessionId = checkoutSessionId;
        StripeCheckoutUrl = checkoutUrl;
        StripeCheckoutUrlExpiresAt = checkoutUrlExpiresAt;
        StripePaymentMethod = paymentMethod;

        if (!string.IsNullOrWhiteSpace(paymentIntentId))
            StripePaymentIntentId = paymentIntentId;
    }

    public void MarkPixMessageSent()
    {
        PixMessageSentAt = DateTime.UtcNow;
        PaymentMessageSentAt = PixMessageSentAt;
    }

    public void MarkPaymentMessageSent()
    {
        PaymentMessageSentAt = DateTime.UtcNow;
    }

    public void SyncStripePaymentStatus(string paymentStatus)
    {
        if (string.IsNullOrWhiteSpace(paymentStatus))
            return;

        StripePaymentStatus = paymentStatus;

        if ((paymentStatus.Equals("succeeded", StringComparison.OrdinalIgnoreCase) ||
             paymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase)) &&
            Status != PaymentStatus.Paid)
        {
            Status = PaymentStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }
    }

    public void AttachStripePaymentIntent(string paymentIntentId)
    {
        if (!string.IsNullOrWhiteSpace(paymentIntentId))
            StripePaymentIntentId = paymentIntentId;
    }

    public void MarkStockDeducted()
    {
        StockDeductedAt ??= DateTime.UtcNow;
    }

    private static DateTime NormalizePaidAt(DateTime? paidAt)
    {
        if (!paidAt.HasValue)
            return DateTime.UtcNow;

        return paidAt.Value.Kind switch
        {
            DateTimeKind.Utc => paidAt.Value,
            DateTimeKind.Local => paidAt.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(paidAt.Value, DateTimeKind.Utc)
        };
    }

    private static void Validate(decimal amount, int number, int total, Guid customerId)
    {
        if (amount <= 0)
            throw new ArgumentException("Invalid amount");

        if (number <= 0 || total <= 0)
            throw new ArgumentException("Invalid installments");

        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer");
    }
}
