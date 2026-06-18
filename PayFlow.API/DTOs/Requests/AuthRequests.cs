namespace PayFlow.API.DTOs.Requests;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? PixKey { get; set; }
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
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateSubscriptionRequest
{
    public bool IsPremium { get; set; }
}
