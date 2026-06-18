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
    public Guid SellerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? AsaasPaymentId { get; set; }
    public string? AsaasPaymentStatus { get; set; }
    public string? AsaasBillingType { get; set; }
    public string? AsaasInvoiceUrl { get; set; }
    public string? AsaasCustomerId { get; set; }
    public string? PixCopyPaste { get; set; }
    public string? PixQrCodeImageUrl { get; set; }
    public string? PixQrCodeSvgUrl { get; set; }
    public string? PixHostedInstructionsUrl { get; set; }
    public DateTime? PixExpiresAt { get; set; }
    public DateTime? PixMessageSentAt { get; set; }
    public DateTime? PaymentMessageSentAt { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public int? CardInstallmentCount { get; set; }
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public DateTime? ManualPaidAt { get; set; }
    public Guid? ManualPaidByUserId { get; set; }
    public string? ManualPaidReason { get; set; }
    public DateTime? ManualPaymentReversedAt { get; set; }
    public Guid? ManualPaymentReversedByUserId { get; set; }
    public string? ManualPaymentReversalReason { get; set; }
    public string? WhatsappMessage { get; set; }
    public string? WhatsappUrl { get; set; }
}

public class PaymentTransactionResponse
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid SellerId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? FeeAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public string Currency { get; set; } = "brl";
    public string? PaymentMethod { get; set; }
    public int? InstallmentCount { get; set; }
    public string? ProviderAccountId { get; set; }
    public string? ProviderEventId { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? ExternalProviderPaymentId { get; set; }
    public string? ExternalChargeId { get; set; }
    public string? ExternalRefundId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorEmail { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
