namespace PayFlow.API.Services;

public class StripeOptions
{
    public string PublishableKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string WebhookSecret { get; set; } = "";
    public long PixExpiresAfterSeconds { get; set; } = 86400;
    public string CheckoutSuccessUrl { get; set; } = "http://localhost:5173/?payment=success";
    public string CheckoutCancelUrl { get; set; } = "http://localhost:5173/?payment=cancelled";

    // URLs used for Stripe Connect onboarding links
    public string AccountLinkSuccessUrl { get; set; } = "http://localhost:5173/?stripe_onboard=success";
    public string AccountLinkRefreshUrl { get; set; } = "http://localhost:5173/?stripe_onboard=refresh";
}
