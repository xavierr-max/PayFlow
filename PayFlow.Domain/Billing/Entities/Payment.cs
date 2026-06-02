using PayFlow.Domain.Sales.Enum;
using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Represents a payment installment.
/// </summary>
public class Payment : Entity
{
    protected Payment() { }

    public Payment(DateTime dueDate, decimal amount, int number, int total, Guid customerId, Guid sellerId)
    {
        Validate(amount, number, total, customerId, sellerId);

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        DueDate = NormalizeDate(dueDate);

        Amount = amount;
        InstallmentNumber = number;
        TotalInstallments = total;
        CustomerId = customerId;
        SellerId = sellerId;
        RefundedAmount = 0;

        TxId = Guid.CreateVersion7().ToString("N");
    }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public decimal Amount { get; private set; }

    public int InstallmentNumber { get; private set; }
    public int TotalInstallments { get; private set; }

    public string TxId { get; private set; } = string.Empty;
    public string? AsaasPaymentId { get; private set; }
    public string? AsaasPaymentStatus { get; private set; }
    public string? AsaasBillingType { get; private set; }
    public string? AsaasInvoiceUrl { get; private set; }
    public string? AsaasCustomerId { get; private set; }
    public string? PixCopyPaste { get; private set; }
    public string? PixQrCodeImageUrl { get; private set; }
    public string? PixQrCodeSvgUrl { get; private set; }
    public string? PixHostedInstructionsUrl { get; private set; }
    public DateTime? PixExpiresAt { get; private set; }
    public DateTime? PixMessageSentAt { get; private set; }
    public DateTime? PaymentMessageSentAt { get; private set; }
    public DateTime? StockDeductedAt { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public decimal? FeeAmount { get; private set; }
    public decimal? NetAmount { get; private set; }
    public int? CardInstallmentCount { get; private set; }
    public string? CardBrand { get; private set; }
    public string? CardLast4 { get; private set; }
    public DateTime? ManualPaidAt { get; private set; }
    public Guid? ManualPaidByUserId { get; private set; }
    public string? ManualPaidReason { get; private set; }
    public PaymentStatus? ManualPaidPreviousStatus { get; private set; }
    public DateTime? ManualPaymentReversedAt { get; private set; }
    public Guid? ManualPaymentReversedByUserId { get; private set; }
    public string? ManualPaymentReversalReason { get; private set; }

    public Guid CustomerId { get; private set; }
    public Guid SellerId { get; private set; }

    public void MarkAsPaid(decimal valueReceived, DateTime? paidAt = null)
    {
        if (valueReceived != Amount)
            throw new InvalidOperationException("Invalid payment amount");

        Status = PaymentStatus.Paid;
        PaidAt = NormalizePaidAt(paidAt);
    }

    public void MarkAsPaidManually(decimal valueReceived, Guid? userId, string? reason, DateTime? paidAt = null)
    {
        if (Status != PaymentStatus.Paid)
            ManualPaidPreviousStatus = Status;

        MarkAsPaid(valueReceived, paidAt);

        ManualPaidAt = DateTime.UtcNow;
        ManualPaidByUserId = userId;
        ManualPaidReason = reason;
        ManualPaymentReversedAt = null;
        ManualPaymentReversedByUserId = null;
        ManualPaymentReversalReason = null;
    }

    public PaymentStatus UndoManualPayment(Guid? userId, string reason)
    {
        if (!ManualPaidAt.HasValue)
            throw new InvalidOperationException("Payment was not manually confirmed");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reversal reason is required");

        if (IsProviderPaidStatus(AsaasPaymentStatus))
            throw new InvalidOperationException("Cannot undo a payment confirmed by the provider");

        var nextStatus = ManualPaidPreviousStatus ?? PaymentStatus.Pending;
        if (nextStatus == PaymentStatus.Paid)
            nextStatus = PaymentStatus.Pending;

        Status = DateTime.UtcNow > DueDate && nextStatus == PaymentStatus.Pending
            ? PaymentStatus.Overdue
            : nextStatus;

        PaidAt = null;
        ManualPaymentReversedAt = DateTime.UtcNow;
        ManualPaymentReversedByUserId = userId;
        ManualPaymentReversalReason = reason;
        return Status;
    }

    public void MarkAsFailed()
    {
        if (Status != PaymentStatus.Paid && Status != PaymentStatus.Refunded)
            Status = PaymentStatus.Failed;
    }

    public void MarkAsCancelled()
    {
        if (Status != PaymentStatus.Paid && Status != PaymentStatus.Refunded)
            Status = PaymentStatus.Cancelled;
    }

    public void MarkAsExpired()
    {
        if (Status == PaymentStatus.Pending || Status == PaymentStatus.Overdue)
            Status = PaymentStatus.Expired;
    }

    public void MarkAsChargeback()
    {
        Status = PaymentStatus.Chargeback;
    }

    public void RegisterRefund(decimal amount, DateTime? refundedAt = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Invalid refund amount");

        RefundedAmount += amount;
        PaidAt ??= NormalizePaidAt(refundedAt);

        Status = RefundedAmount >= Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartiallyRefunded;
    }

    public void UpdateDueDate(DateTime dueDate)
    {
        DueDate = NormalizeDate(dueDate);

        if (Status == PaymentStatus.Overdue && DateTime.UtcNow <= DueDate)
            Status = PaymentStatus.Pending;
    }

    public void MarkAsOverdue()
    {
        if (Status == PaymentStatus.Pending && DateTime.UtcNow > DueDate)
            Status = PaymentStatus.Overdue;
    }

    public bool HasActivePix(DateTime utcNow)
    {
        return AsaasBillingType?.Equals("PIX", StringComparison.OrdinalIgnoreCase) == true &&
               !string.IsNullOrWhiteSpace(AsaasPaymentId) &&
               !string.IsNullOrWhiteSpace(PixCopyPaste) &&
               (!PixExpiresAt.HasValue || PixExpiresAt.Value > utcNow);
    }

    public bool HasActivePaymentLink(string billingType)
    {
        return AsaasBillingType?.Equals(billingType, StringComparison.OrdinalIgnoreCase) == true &&
               !string.IsNullOrWhiteSpace(AsaasPaymentId) &&
               !string.IsNullOrWhiteSpace(AsaasInvoiceUrl) &&
               Status is PaymentStatus.Pending or PaymentStatus.Overdue;
    }

    public void AttachAsaasPayment(
        string paymentId,
        string paymentStatus,
        string billingType,
        string asaasCustomerId,
        string? invoiceUrl,
        string? pixCopyPaste = null,
        string? pixQrCodeImageUrl = null,
        string? pixQrCodeSvgUrl = null,
        DateTime? pixExpiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
            throw new ArgumentException("Invalid Asaas payment ID");

        if (string.IsNullOrWhiteSpace(billingType))
            throw new ArgumentException("Invalid Asaas billing type");

        AsaasPaymentId = paymentId.Trim();
        AsaasPaymentStatus = string.IsNullOrWhiteSpace(paymentStatus) ? "PENDING" : paymentStatus.Trim();
        AsaasBillingType = billingType.Trim().ToUpperInvariant();
        AsaasCustomerId = string.IsNullOrWhiteSpace(asaasCustomerId) ? AsaasCustomerId : asaasCustomerId.Trim();
        AsaasInvoiceUrl = invoiceUrl;

        PixCopyPaste = pixCopyPaste;
        PixQrCodeImageUrl = pixQrCodeImageUrl;
        PixQrCodeSvgUrl = pixQrCodeSvgUrl;
        PixHostedInstructionsUrl = AsaasBillingType == "PIX" ? invoiceUrl : null;
        PixExpiresAt = pixExpiresAt;

        SyncAsaasPaymentStatus(AsaasPaymentStatus);
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

    public void SyncAsaasPaymentStatus(string? paymentStatus, DateTime? paidAt = null)
    {
        if (string.IsNullOrWhiteSpace(paymentStatus))
            return;

        AsaasPaymentStatus = paymentStatus.Trim();
        var normalized = AsaasPaymentStatus.ToUpperInvariant();

        if (IsProviderPaidStatus(normalized))
        {
            Status = PaymentStatus.Paid;
            PaidAt ??= NormalizePaidAt(paidAt);
            return;
        }

        if (normalized is "CANCELED" or "CANCELLED" or "PAYMENT_DELETED")
        {
            MarkAsCancelled();
            return;
        }

        if (normalized is "OVERDUE")
        {
            MarkAsOverdue();
            return;
        }

        if (normalized is "REFUNDED")
        {
            RegisterRefund(Amount, paidAt);
            return;
        }

        if (normalized is "REFUND_REQUESTED")
        {
            if (Status != PaymentStatus.Paid)
                MarkAsFailed();

            return;
        }

        if (normalized is "CHARGEBACK_REQUESTED" or "CHARGEBACK_DISPUTE" or "AWAITING_CHARGEBACK_REVERSAL")
            MarkAsChargeback();
    }

    public void AttachAsaasFinancialDetails(
        decimal? feeAmount,
        decimal? netAmount,
        int? cardInstallmentCount = null,
        string? cardBrand = null,
        string? cardLast4 = null)
    {
        FeeAmount = feeAmount;
        NetAmount = netAmount;
        CardInstallmentCount = cardInstallmentCount;
        CardBrand = cardBrand;
        CardLast4 = cardLast4;
    }

    public void MarkStockDeducted()
    {
        StockDeductedAt ??= DateTime.UtcNow;
    }

    private static bool IsProviderPaidStatus(string? paymentStatus)
    {
        return paymentStatus?.Trim().ToUpperInvariant() is "RECEIVED" or "CONFIRMED" or "RECEIVED_IN_CASH" or "PAID";
    }

    private static DateTime NormalizePaidAt(DateTime? paidAt)
    {
        if (!paidAt.HasValue)
            return DateTime.UtcNow;

        return NormalizeDate(paidAt.Value);
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

    private static void Validate(decimal amount, int number, int total, Guid customerId, Guid sellerId)
    {
        if (amount <= 0)
            throw new ArgumentException("Invalid amount");

        if (number <= 0 || total <= 0)
            throw new ArgumentException("Invalid installments");

        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");
    }
}
