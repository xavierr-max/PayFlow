namespace PayFlow.API.DTOs.Responses;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string StatusLabel { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentNumber { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string TxId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
}

public class CategoryResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
}
