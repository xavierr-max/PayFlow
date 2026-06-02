namespace PayFlow.Domain.Sales.Enum;

/// <summary>
/// Payment status.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    Failed = 5,
    Expired = 6,
    Refunded = 7,
    PartiallyRefunded = 8,
    Chargeback = 9
}
