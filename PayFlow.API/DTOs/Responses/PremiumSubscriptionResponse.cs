namespace PayFlow.API.DTOs.Responses;

public class PremiumCheckoutResponse
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}

public class PremiumSubscriptionResponse
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SellerId { get; set; }
    public string Status { get; set; } = "free";
    public string StatusLabel { get; set; } = "Gratuito";
    public bool IsPremium { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    public string? ProviderCheckoutUrl { get; set; }
}

public class PremiumSubscriptionPaymentResponse
{
    public Guid Id { get; set; }
    public Guid PremiumSubscriptionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "brl";
    public string? ExternalInvoiceId { get; set; }
    public string? ExternalProviderPaymentId { get; set; }
    public string? ExternalChargeId { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
