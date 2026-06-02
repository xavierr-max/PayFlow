using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Auth;
using PayFlow.API.Exceptions;
using PayFlow.API.Services;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly, CancellationToken cancellationToken)
    {
        var sellerId = GetSellerId();
        var notifications = await _notificationService.GetNotificationsAsync(sellerId, unreadOnly, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _notificationService.GetSummaryAsync(GetSellerId(), cancellationToken);
        return Ok(summary);
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(GetSellerId(), notificationId, cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(GetSellerId(), cancellationToken);
        return NoContent();
    }

    private Guid GetSellerId()
    {
        return User.GetSellerId() ?? throw new UnauthorizedException("Usuário não autenticado");
    }
}
