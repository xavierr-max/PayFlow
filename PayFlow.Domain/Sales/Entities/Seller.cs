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

        Name = name.Trim();
        StoreName = storeName.Trim();
        PixKey = pixKey?.Trim() ?? "";
        AsaasSubaccountStatus = "pending_data";
    }

    public string Name { get; private set; } = string.Empty;
    public string StoreName { get; private set; } = string.Empty;
    public string PixKey { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? CpfCnpj { get; private set; }
    public string? MobilePhone { get; private set; }
    public string? Phone { get; private set; }
    public decimal? IncomeValue { get; private set; }
    public string? CompanyType { get; private set; }
    public string? Address { get; private set; }
    public string? AddressNumber { get; private set; }
    public string? Complement { get; private set; }
    public string? Province { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Site { get; private set; }
    public string? AsaasAccountId { get; private set; }
    public string? AsaasWalletId { get; private set; }
    public string? AsaasApiKey { get; private set; }
    public string AsaasSubaccountStatus { get; private set; } = "pending_data";

    public void Update(string name, string storeName, string pixKey)
    {
        Validate(name, storeName, pixKey);

        Name = name.Trim();
        StoreName = storeName.Trim();
        PixKey = pixKey?.Trim() ?? "";
    }

    public void UpdateAsaasProfile(
        string? email,
        string? cpfCnpj,
        string? mobilePhone,
        decimal? incomeValue,
        string? address,
        string? addressNumber,
        string? province,
        string? postalCode,
        string? companyType = null,
        string? phone = null,
        string? complement = null,
        string? site = null)
    {
        Email = Normalize(email);
        CpfCnpj = OnlyDigits(cpfCnpj);
        MobilePhone = OnlyDigits(mobilePhone);
        IncomeValue = incomeValue;
        Address = Normalize(address);
        AddressNumber = Normalize(addressNumber);
        Province = Normalize(province);
        PostalCode = OnlyDigits(postalCode);
        CompanyType = Normalize(companyType)?.ToUpperInvariant();
        Phone = OnlyDigits(phone);
        Complement = Normalize(complement);
        Site = Normalize(site);

        if (!HasAsaasSubaccount())
            AsaasSubaccountStatus = HasRequiredAsaasSubaccountData() ? "ready_to_create" : "pending_data";
    }

    public void SetAsaasSubaccount(string accountId, string walletId, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Asaas account ID cannot be empty");

        if (string.IsNullOrWhiteSpace(walletId))
            throw new ArgumentException("Asaas wallet ID cannot be empty");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Asaas API key cannot be empty");

        AsaasAccountId = accountId.Trim();
        AsaasWalletId = walletId.Trim();
        AsaasApiKey = apiKey.Trim();
        AsaasSubaccountStatus = "active";
    }

    public bool HasAsaasSubaccount()
    {
        return !string.IsNullOrWhiteSpace(AsaasAccountId) &&
               !string.IsNullOrWhiteSpace(AsaasWalletId) &&
               !string.IsNullOrWhiteSpace(AsaasApiKey);
    }

    public bool HasRequiredAsaasSubaccountData()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(CpfCnpj) &&
               CpfCnpj.Length == 14 &&
               !string.IsNullOrWhiteSpace(MobilePhone) &&
               IncomeValue is > 0 &&
               !string.IsNullOrWhiteSpace(CompanyType) &&
               !string.IsNullOrWhiteSpace(Address) &&
               !string.IsNullOrWhiteSpace(AddressNumber) &&
               !string.IsNullOrWhiteSpace(Province) &&
               !string.IsNullOrWhiteSpace(PostalCode);
    }

    public IReadOnlyList<string> GetMissingAsaasSubaccountFields()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(Email))
            missing.Add("email");

        if (string.IsNullOrWhiteSpace(CpfCnpj) || CpfCnpj.Length != 14)
            missing.Add("cpfCnpj com CNPJ válido");

        if (string.IsNullOrWhiteSpace(MobilePhone))
            missing.Add("mobilePhone");

        if (IncomeValue is null or <= 0)
            missing.Add("incomeValue");

        if (string.IsNullOrWhiteSpace(CompanyType))
            missing.Add("companyType");

        if (string.IsNullOrWhiteSpace(Address))
            missing.Add("address");

        if (string.IsNullOrWhiteSpace(AddressNumber))
            missing.Add("addressNumber");

        if (string.IsNullOrWhiteSpace(Province))
            missing.Add("province");

        if (string.IsNullOrWhiteSpace(PostalCode))
            missing.Add("postalCode");

        return missing;
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
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }
}
