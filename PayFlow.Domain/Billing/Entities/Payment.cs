using PayFlow.Domain.Sales.Enum;
using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Represents a payment installment.
/// </summary>
public class Payment : Entity
{
    protected Payment() { }

    public Payment(DateTime dueDate, decimal amount, int number, int total, Guid customerId)
    {
        Validate(amount, number, total, customerId);

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        DueDate = dueDate;

        Amount = amount;
        InstallmentNumber = number;
        TotalInstallments = total;
        CustomerId = customerId;

        TxId = Guid.CreateVersion7().ToString("N");
    }

    public PaymentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public decimal Amount { get; private set; }

    public int InstallmentNumber { get; private set; }
    public int TotalInstallments { get; private set; }

    public string TxId { get; private set; }

    public Guid CustomerId { get; private set; }

    public void MarkAsPaid(decimal valueReceived)
    {
        if (valueReceived != Amount)
            throw new InvalidOperationException("Invalid payment amount");

        Status = PaymentStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsOverdue()
    {
        if (Status == PaymentStatus.Pending && DateTime.UtcNow > DueDate)
        {
            Status = PaymentStatus.Overdue;
        }
    }

    private static void Validate(decimal amount, int number, int total, Guid customerId)
    {
        if (amount <= 0)
            throw new ArgumentException("Invalid amount");

        if (number <= 0 || total <= 0)
            throw new ArgumentException("Invalid installments");

        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer");
    }
}