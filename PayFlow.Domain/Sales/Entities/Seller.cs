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
        ConnectedAccountId = null;
    }

    public string Name { get; private set; }
    public string StoreName { get; private set; }
    public string PixKey { get; private set; }
    public string? ConnectedAccountId { get; private set; }

    public void Update(string name, string storeName, string pixKey, string? connectedAccountId = null)
    {
        Validate(name, storeName, pixKey);

        Name = name;
        StoreName = storeName;
        PixKey = pixKey;
        if (connectedAccountId != null)
            ConnectedAccountId = connectedAccountId;
    }

    public void SetConnectedAccountId(string connectedAccountId)
    {
        if (string.IsNullOrWhiteSpace(connectedAccountId))
            throw new ArgumentException("Connected account ID cannot be empty");
        ConnectedAccountId = connectedAccountId;
    }

    public bool HasConnectedAccount()
    {
        return !string.IsNullOrWhiteSpace(ConnectedAccountId);
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