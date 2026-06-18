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

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? CpfCnpj { get; private set; }
    public string? Email { get; private set; }
    public string? AsaasCustomerId { get; private set; }
    public Guid SellerId { get; private set; }

    public void Update(string name, string description, string phone)
    {
        Validate(name, phone, SellerId);

        Name = name;
        Description = description;
        Phone = phone;
    }

    public void UpdateAsaasProfile(string? cpfCnpj, string? email)
    {
        CpfCnpj = OnlyDigits(cpfCnpj);
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public void SetAsaasCustomerId(string asaasCustomerId)
    {
        if (string.IsNullOrWhiteSpace(asaasCustomerId))
            throw new ArgumentException("Asaas customer ID cannot be empty");

        AsaasCustomerId = asaasCustomerId.Trim();
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

    private static string? OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }
}
