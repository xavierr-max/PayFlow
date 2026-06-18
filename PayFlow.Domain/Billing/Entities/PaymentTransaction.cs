using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Immutable financial ledger entry for a payment lifecycle event.
/// </summary>
public class PaymentTransaction : Entity
{
    protected PaymentTransaction() { }

    public PaymentTransaction(
        Guid paymentId,
        Guid sellerId,
        string provider,
        string type,
        string status,
        decimal amount,
        string currency,
        string? paymentMethod = null,
        decimal? feeAmount = null,
        decimal? netAmount = null,
        int? installmentCount = null,
        string? providerAccountId = null,
        string? providerEventId = null,
        string? externalTransactionId = null,
        string? externalProviderPaymentId = null,
        string? externalChargeId = null,
        string? externalRefundId = null,
        Guid? actorUserId = null,
        string? actorEmail = null,
        string? reason = null,
        string? notes = null,
        DateTime? occurredAt = null)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Invalid payment");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required");

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Transaction type is required");

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Transaction status is required");

        PaymentId = paymentId;
        SellerId = sellerId;
        Provider = provider.Trim();
        Type = type.Trim();
        Status = status.Trim();
        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "brl" : currency.Trim().ToLowerInvariant();
        PaymentMethod = paymentMethod;
        FeeAmount = feeAmount;
        NetAmount = netAmount;
        InstallmentCount = installmentCount;
        ProviderAccountId = providerAccountId;
        ProviderEventId = providerEventId;
        ExternalTransactionId = externalTransactionId;
        ExternalProviderPaymentId = externalProviderPaymentId;
        ExternalChargeId = externalChargeId;
        ExternalRefundId = externalRefundId;
        ActorUserId = actorUserId;
        ActorEmail = actorEmail;
        Reason = reason;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        OccurredAt = NormalizeDate(occurredAt ?? CreatedAt);
    }

    public Guid PaymentId { get; private set; }
    public Guid SellerId { get; private set; }

    public string Provider { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }
    public decimal? FeeAmount { get; private set; }
    public decimal? NetAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public string? PaymentMethod { get; private set; }
    public int? InstallmentCount { get; private set; }
    public string? ProviderAccountId { get; private set; }
    public string? ProviderEventId { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalProviderPaymentId { get; private set; }
    public string? ExternalChargeId { get; private set; }
    public string? ExternalRefundId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string? ActorEmail { get; private set; }
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime OccurredAt { get; private set; }

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
