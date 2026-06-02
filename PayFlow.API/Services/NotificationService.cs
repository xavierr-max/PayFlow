using Microsoft.EntityFrameworkCore;
using PayFlow.API.Data;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Notifications.Entities;

namespace PayFlow.API.Services;

public interface INotificationService
{
    void NotifySeller(
        Guid sellerId,
        string type,
        string title,
        string message,
        Guid? userId = null,
        string? targetUrl = null,
        string? deduplicationKey = null);

    Task<List<NotificationResponse>> GetNotificationsAsync(Guid sellerId, bool unreadOnly, CancellationToken cancellationToken);
    Task<NotificationSummaryResponse> GetSummaryAsync(Guid sellerId, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid sellerId, Guid notificationId, CancellationToken cancellationToken);
    Task MarkAllAsReadAsync(Guid sellerId, CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;

    public NotificationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void NotifySeller(
        Guid sellerId,
        string type,
        string title,
        string message,
        Guid? userId = null,
        string? targetUrl = null,
        string? deduplicationKey = null)
    {
        if (!string.IsNullOrWhiteSpace(deduplicationKey) &&
            _dbContext.Notifications.Any(notification => notification.DeduplicationKey == deduplicationKey))
        {
            return;
        }

        _dbContext.Notifications.Add(new Notification(
            sellerId,
            type,
            title,
            message,
            userId,
            targetUrl,
            deduplicationKey));

        _dbContext.SaveChanges();
    }

    public async Task<List<NotificationResponse>> GetNotificationsAsync(Guid sellerId, bool unreadOnly, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .Where(notification => notification.SellerId == sellerId);

        if (unreadOnly)
            query = query.Where(notification => !notification.IsRead);

        return await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(100)
            .Select(notification => Map(notification))
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationSummaryResponse> GetSummaryAsync(Guid sellerId, CancellationToken cancellationToken)
    {
        return new NotificationSummaryResponse
        {
            UnreadCount = await _dbContext.Notifications.CountAsync(
                notification => notification.SellerId == sellerId && !notification.IsRead,
                cancellationToken)
        };
    }

    public async Task MarkAsReadAsync(Guid sellerId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(
            item => item.Id == notificationId && item.SellerId == sellerId,
            cancellationToken);

        if (notification == null)
            throw new NotFoundException("Notificação não encontrada");

        notification.MarkAsRead();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid sellerId, CancellationToken cancellationToken)
    {
        var notifications = await _dbContext.Notifications
            .Where(notification => notification.SellerId == sellerId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
            notification.MarkAsRead();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationResponse Map(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            SellerId = notification.SellerId,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            TargetUrl = notification.TargetUrl,
            Channels = notification.Channels,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}
