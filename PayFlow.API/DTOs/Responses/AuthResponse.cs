namespace PayFlow.API.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserResponse User { get; set; } = new();
}

public class AuthUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public bool IsPremium { get; set; }
}
