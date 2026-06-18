namespace PayFlow.API.Services;

public class AsaasOptions
{
    public string BaseUrl { get; set; } = "https://api-sandbox.asaas.com/v3";
    public string ApiKey { get; set; } = "";
    public string UserAgent { get; set; } = "PayFlow/1.0";
    public string WebhookUrl { get; set; } = "";
    public string WebhookEmail { get; set; } = "";
    public string WebhookAuthToken { get; set; } = "";
    public decimal PremiumMonthlyAmount { get; set; } = 19.99m;
    public string PremiumBillingType { get; set; } = "CREDIT_CARD";
    public string PremiumCycle { get; set; } = "MONTHLY";
}
