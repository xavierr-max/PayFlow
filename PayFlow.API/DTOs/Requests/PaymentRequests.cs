namespace PayFlow.API.DTOs.Requests;

public class CreatePaymentRequest
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentNumber { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime DueDate { get; set; }
    public List<CreatePaymentItemRequest> Items { get; set; } = new();
}

public class CreatePaymentItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class MarkPaymentPaidRequest
{
    public decimal ValueReceived { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Reason { get; set; }
}

public class UnmarkPaymentPaidRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CancelPaymentRequest
{
    public string? Reason { get; set; }
}

public class RefundPaymentRequest
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class UpdatePaymentDatesRequest
{
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreatePaymentWhatsappMessageRequest
{
    public string PaymentMethod { get; set; } = "pix";
}
