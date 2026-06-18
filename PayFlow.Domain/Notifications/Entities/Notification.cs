using PayFlow.Domain.Shared.Entities;

namespace PayFlow.Domain.Notifications.Entities;

/// <summary>
/// In-app notification with channel metadata for future email/WhatsApp delivery.
/// </summary>
public class Notification : Entity
{
    protected Notification() { }

    public Notification(
        Guid sellerId,
        string type,
        string title,
        string message,
        Guid? userId = null,
        string? targetUrl = null,
        string? deduplicationKey = null,
        string channels = "in_app")
    {
        if (sellerId == Guid.Empty)
            throw new ArgumentException("Invalid seller");

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Notification type is required");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title is required");

        SellerId = sellerId;
        UserId = userId;
        Type = type.Trim();
        Title = title.Trim();
        Message = message.Trim();
        TargetUrl = targetUrl;
        DeduplicationKey = deduplicationKey;
        Channels = string.IsNullOrWhiteSpace(channels) ? "in_app" : channels.Trim();
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
    }

    public Guid SellerId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? TargetUrl { get; private set; }
    public string? DeduplicationKey { get; private set; }
    public string Channels { get; private set; } = "in_app";
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
