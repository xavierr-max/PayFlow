namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Represents a product snapshot inside a payment.
/// </summary>
public class PaymentItem
{
    public PaymentItem(Guid productId, Guid paymentId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Invalid quantity");

        if (unitPrice <= 0)
            throw new ArgumentException("Invalid price");

        ProductId = productId;
        PaymentId = paymentId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid ProductId { get; private set; }
    public Guid PaymentId { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
}