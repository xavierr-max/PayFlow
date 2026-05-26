using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Sales.Entities;

/// <summary>
/// Represents a product owned by a seller.
/// </summary>
public class Product : Entity
{
    protected Product() { }

    public Product(string name, string description, decimal price, int quantity, Guid sellerId)
    {
        Validate(name, price, quantity, sellerId);

        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        SellerId = sellerId;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public Guid SellerId { get; private set; }

    public void Update(string name, string description, decimal price, int quantity)
    {
        Validate(name, price, quantity, SellerId);

        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
    }

    private static void Validate(string name, decimal price, int quantity, Guid sellerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (price <= 0)
            throw new ArgumentException("Invalid price");

        if (quantity < 0)
            throw new ArgumentException("Invalid quantity");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");
    }
}