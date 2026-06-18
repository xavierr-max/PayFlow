namespace PayFlow.API.DTOs.Responses;

public class SellerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string PixKey { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? CpfCnpj { get; set; }
    public string? MobilePhone { get; set; }
    public string? Phone { get; set; }
    public decimal? IncomeValue { get; set; }
    public string? CompanyType { get; set; }
    public string? Address { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Site { get; set; }
    public string? AsaasAccountId { get; set; }
    public string? AsaasWalletId { get; set; }
    public bool HasAsaasApiKey { get; set; }
    public bool IsAsaasReady { get; set; }
    public string AsaasSubaccountStatus { get; set; } = string.Empty;
    public List<string> MissingAsaasFields { get; set; } = new();
}

public class AsaasAccountStatusResponse
{
    public Guid SellerId { get; set; }
    public string? AsaasAccountId { get; set; }
    public string? AsaasWalletId { get; set; }
    public bool HasApiKey { get; set; }
    public bool IsReady { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> MissingFields { get; set; } = new();
}

public class SellerAsaasAccountResponse
{
    public Guid SellerId { get; set; }
    public string AsaasAccountId { get; set; } = string.Empty;
    public string AsaasWalletId { get; set; } = string.Empty;
    public AsaasAccountStatusResponse Status { get; set; } = new();
}
