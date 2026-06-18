namespace PayFlow.API.Auth;

public class JwtOptions
{
    public string Key { get; set; } = "payflow-development-key-change-before-production-2026";
    public string Issuer { get; set; } = "PayFlow";
    public string Audience { get; set; } = "PayFlow";
    public int ExpiresMinutes { get; set; } = 10080;
}
