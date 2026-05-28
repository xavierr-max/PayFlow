using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Sales.Entities;

/// <summary>
/// Represents a product owned by a seller.
/// </summary>
public class Product : Entity
{
    protected Product() { }

    public Product(string name, string description, decimal price, int quantity, Guid sellerId, string categoryId, string imageUrl)
    {
        Validate(name, price, quantity, sellerId);

        Name = name.Trim();
        Description = description;
        Price = price;
        Quantity = quantity;
        SellerId = sellerId;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public Guid SellerId { get; private set; }
    public string CategoryId { get; private set; }
    public string ImageUrl { get; private set; }

    public void Update(string name, string description, decimal price, int quantity, string categoryId, string imageUrl)
    {
        Validate(name, price, quantity, SellerId);

        Name = name.Trim();
        Description = description;
        Price = price;
        Quantity = quantity;
        CategoryId = categoryId;
        ImageUrl = imageUrl;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Invalid quantity");

        Quantity = Math.Max(0, Quantity - quantity);
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
