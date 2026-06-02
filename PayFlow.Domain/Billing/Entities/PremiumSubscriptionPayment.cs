using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

public class PremiumSubscriptionPayment : Entity
{
    protected PremiumSubscriptionPayment() { }

    public PremiumSubscriptionPayment(
        Guid premiumSubscriptionId,
        Guid userId,
        Guid sellerId,
        string provider,
        string status,
        decimal amount,
        string currency,
        string? externalInvoiceId = null,
        string? externalProviderPaymentId = null,
        string? externalChargeId = null,
        DateTime? paidAt = null,
        string? failureReason = null)
    {
        if (premiumSubscriptionId == Guid.Empty)
            throw new ArgumentException("Invalid subscription");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");

        PremiumSubscriptionId = premiumSubscriptionId;
        UserId = userId;
        SellerId = sellerId;
        Provider = string.IsNullOrWhiteSpace(provider) ? "asaas" : provider.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? "unknown" : status.Trim();
        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "brl" : currency.Trim().ToLowerInvariant();
        ExternalInvoiceId = externalInvoiceId;
        ExternalProviderPaymentId = externalProviderPaymentId;
        ExternalChargeId = externalChargeId;
        PaidAt = NormalizeDate(paidAt);
        FailureReason = failureReason;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PremiumSubscriptionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SellerId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? ExternalInvoiceId { get; private set; }
    public string? ExternalProviderPaymentId { get; private set; }
    public string? ExternalChargeId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private static DateTime? NormalizeDate(DateTime? date)
    {
        if (!date.HasValue)
            return null;

        return date.Value.Kind switch
        {
            DateTimeKind.Utc => date.Value,
            DateTimeKind.Local => date.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)
        };
    }
}
