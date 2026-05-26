using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Sales.Entities;

/// <summary>
/// Represents a customer linked to a seller.
/// </summary>
public class Customer : Entity
{
    protected Customer() { }

    public Customer(string name, string description, string phone, Guid sellerId)
    {
        Validate(name, phone, sellerId);

        Name = name;
        Description = description;
        Phone = phone;
        SellerId = sellerId;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Phone { get; private set; }
    public Guid SellerId { get; private set; }

    public void Update(string name, string description, string phone)
    {
        Validate(name, phone, SellerId);

        Name = name;
        Description = description;
        Phone = phone;
    }

    private static void Validate(string name, string phone, Guid sellerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required");

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");
    }
}