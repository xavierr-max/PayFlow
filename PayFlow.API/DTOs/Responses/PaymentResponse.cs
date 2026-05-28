namespace PayFlow.API.DTOs.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int InstallmentNumber { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string TxId { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public string? StripePaymentStatus { get; set; }
    public string? PixCopyPaste { get; set; }
    public string? PixQrCodeImageUrl { get; set; }
    public string? PixQrCodeSvgUrl { get; set; }
    public string? PixHostedInstructionsUrl { get; set; }
    public DateTime? PixExpiresAt { get; set; }
    public DateTime? PixMessageSentAt { get; set; }
    public string? StripePaymentMethod { get; set; }
    public string? StripeCheckoutSessionId { get; set; }
    public string? StripeCheckoutUrl { get; set; }
    public DateTime? StripeCheckoutUrlExpiresAt { get; set; }
    public DateTime? PaymentMessageSentAt { get; set; }
    public string? WhatsappMessage { get; set; }
    public string? WhatsappUrl { get; set; }
}

public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
