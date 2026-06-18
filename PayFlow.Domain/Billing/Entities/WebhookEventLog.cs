using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Billing.Entities;

/// <summary>
/// Audit log for inbound provider webhooks.
/// </summary>
public class WebhookEventLog : Entity
{
    protected WebhookEventLog() { }

    public WebhookEventLog(string provider, string eventId, string eventType, string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required");

        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("Webhook event ID is required");

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Webhook event type is required");

        Provider = provider.Trim();
        EventId = eventId.Trim();
        EventType = eventType.Trim();
        RawPayload = rawPayload;
        ProcessingStatus = "received";
        ReceivedAt = DateTime.UtcNow;
    }

    public string Provider { get; private set; } = string.Empty;
    public string EventId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string RawPayload { get; private set; } = string.Empty;
    public string ProcessingStatus { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public Guid? PaymentId { get; private set; }
    public Guid? SellerId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public bool IsProcessed => ProcessedAt.HasValue && ProcessingStatus == "processed";

    public void MarkProcessed(Guid? paymentId = null, Guid? sellerId = null)
    {
        PaymentId = paymentId;
        SellerId = sellerId;
        ProcessingStatus = "processed";
        ErrorMessage = null;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage, Guid? paymentId = null, Guid? sellerId = null)
    {
        PaymentId = paymentId;
        SellerId = sellerId;
        ProcessingStatus = "failed";
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Unknown webhook processing error" : errorMessage;
        ProcessedAt = DateTime.UtcNow;
    }
}
