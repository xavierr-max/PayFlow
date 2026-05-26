using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Sales.Entities;

/// <summary>
/// Represents a seller in the system.
/// </summary>
public class Seller : Entity
{
    protected Seller() { }

    public Seller(string name, string storeName, string pixKey)
    {
        Validate(name, storeName, pixKey);

        Name = name;
        StoreName = storeName;
        PixKey = pixKey;
    }

    public string Name { get; private set; }
    public string StoreName { get; private set; }
    public string PixKey { get; private set; }

    public void Update(string name, string storeName, string pixKey)
    {
        Validate(name, storeName, pixKey);

        Name = name;
        StoreName = storeName;
        PixKey = pixKey;
    }

    public bool HasPixKey()
    {
        return !string.IsNullOrWhiteSpace(PixKey);
    }

    private static void Validate(string name, string storeName, string pixKey)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(storeName))
            throw new ArgumentException("Store name is required");

        if (string.IsNullOrWhiteSpace(pixKey))
            throw new ArgumentException("Pix key is required");
    }
}