namespace PayFlow.API.DTOs.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid? UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetUrl { get; set; }
    public string Channels { get; set; } = "in_app";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationSummaryResponse
{
    public int UnreadCount { get; set; }
}
