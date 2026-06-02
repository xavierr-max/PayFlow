using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Auth;
using PayFlow.API.DTOs.Responses;
using PayFlow.API.Exceptions;
using PayFlow.API.Services;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/premium")]
[Authorize]
public class PremiumController : ControllerBase
{
    private readonly IPremiumSubscriptionService _premiumSubscriptionService;

    public PremiumController(IPremiumSubscriptionService premiumSubscriptionService)
    {
        _premiumSubscriptionService = premiumSubscriptionService;
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<PremiumSubscriptionResponse>> GetSubscription(CancellationToken cancellationToken)
    {
        var (userId, sellerId) = GetIdentity();
        return Ok(await _premiumSubscriptionService.GetSubscriptionAsync(userId, sellerId, cancellationToken));
    }

    [HttpGet("payments")]
    public async Task<ActionResult<List<PremiumSubscriptionPaymentResponse>>> GetPayments(CancellationToken cancellationToken)
    {
        var (userId, sellerId) = GetIdentity();
        return Ok(await _premiumSubscriptionService.GetPaymentsAsync(userId, sellerId, cancellationToken));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<PremiumCheckoutResponse>> CreatePremiumCheckout(CancellationToken cancellationToken)
    {
        var (userId, sellerId) = GetIdentity();
        return Ok(await _premiumSubscriptionService.CreatePremiumCheckoutAsync(
            userId,
            sellerId,
            User.GetEmail(),
            cancellationToken));
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<PremiumSubscriptionResponse>> Cancel(CancellationToken cancellationToken)
    {
        var (userId, sellerId) = GetIdentity();
        return Ok(await _premiumSubscriptionService.CancelAsync(userId, sellerId, cancellationToken));
    }

    [HttpPost("reactivate")]
    public async Task<ActionResult<PremiumSubscriptionResponse>> Reactivate(CancellationToken cancellationToken)
    {
        var (userId, sellerId) = GetIdentity();
        return Ok(await _premiumSubscriptionService.ReactivateAsync(userId, sellerId, cancellationToken));
    }

    private (Guid UserId, Guid SellerId) GetIdentity()
    {
        var userId = User.GetUserId();
        var sellerId = User.GetSellerId();

        if (userId == null || sellerId == null)
            throw new UnauthorizedException("Usuário não autenticado");

        return (userId.Value, sellerId.Value);
    }
}
