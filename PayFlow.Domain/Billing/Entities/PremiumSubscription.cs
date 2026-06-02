using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

public class PremiumSubscription : Entity
{
    protected PremiumSubscription() { }

    public PremiumSubscription(Guid userId, Guid sellerId, string provider)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");

        UserId = userId;
        SellerId = sellerId;
        Provider = string.IsNullOrWhiteSpace(provider) ? "asaas" : provider.Trim();
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; private set; }
    public Guid SellerId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? ProviderCustomerId { get; private set; }
    public string? ProviderSubscriptionId { get; private set; }
    public string? ProviderCheckoutUrl { get; private set; }
    public bool CancelAtPeriodEnd { get; private set; }
    public DateTime? CurrentPeriodStart { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsActive => Status == "active";

    public void AttachProviderCustomer(string providerCustomerId)
    {
        if (!string.IsNullOrWhiteSpace(providerCustomerId))
            ProviderCustomerId = providerCustomerId.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void AttachProviderSubscription(
        string providerSubscriptionId,
        string? providerCustomerId,
        string? checkoutUrl,
        DateTime? currentPeriodStart,
        DateTime? currentPeriodEnd)
    {
        if (!string.IsNullOrWhiteSpace(providerSubscriptionId))
            ProviderSubscriptionId = providerSubscriptionId.Trim();

        if (!string.IsNullOrWhiteSpace(providerCustomerId))
            ProviderCustomerId = providerCustomerId.Trim();

        if (!string.IsNullOrWhiteSpace(checkoutUrl))
            ProviderCheckoutUrl = checkoutUrl.Trim();

        CurrentPeriodStart = NormalizeDate(currentPeriodStart);
        CurrentPeriodEnd = NormalizeDate(currentPeriodEnd);
        CancelAtPeriodEnd = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncProviderStatus(
        string status,
        DateTime? currentPeriodStart,
        DateTime? currentPeriodEnd,
        bool cancelAtPeriodEnd,
        DateTime? cancelledAt)
    {
        Status = NormalizeStatus(status);
        CurrentPeriodStart = NormalizeDate(currentPeriodStart);
        CurrentPeriodEnd = NormalizeDate(currentPeriodEnd);
        CancelAtPeriodEnd = cancelAtPeriodEnd;
        CancelledAt = NormalizeDate(cancelledAt);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkActive(DateTime? currentPeriodStart = null, DateTime? currentPeriodEnd = null)
    {
        Status = "active";
        CurrentPeriodStart = NormalizeDate(currentPeriodStart) ?? DateTime.UtcNow;
        CurrentPeriodEnd = NormalizeDate(currentPeriodEnd) ?? CurrentPeriodStart?.AddMonths(1);
        CancelAtPeriodEnd = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = "cancelled";
        CancelAtPeriodEnd = false;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReactivated()
    {
        if (Status == "cancelled")
            Status = "pending";

        CancelAtPeriodEnd = false;
        CancelledAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPastDue()
    {
        Status = "past_due";
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "ACTIVE" => "active",
            "RECEIVED" => "active",
            "CONFIRMED" => "active",
            "PAID" => "active",
            "OVERDUE" => "past_due",
            "PENDING" => "pending",
            "CANCELED" => "cancelled",
            "CANCELLED" => "cancelled",
            "DELETED" => "cancelled",
            "EXPIRED" => "expired",
            _ => "pending"
        };
    }

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
