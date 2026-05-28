namespace PayFlow.API.DTOs.Requests;

public class CreatePaymentRequest
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public int InstallmentNumber { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime DueDate { get; set; }
}

public class MarkPaymentPaidRequest
{
    public decimal ValueReceived { get; set; }
}
