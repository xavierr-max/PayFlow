using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Services;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/asaas")]
public class AsaasWebhookController : ControllerBase
{
    private readonly IAsaasWebhookService _asaasWebhookService;

    public AsaasWebhookController(IAsaasWebhookService asaasWebhookService)
    {
        _asaasWebhookService = asaasWebhookService;
    }

    [HttpPost("webhook")]
    [HttpPost("/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        var body = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var result = await _asaasWebhookService.ProcessAsync(
            body,
            Request.Headers["asaas-access-token"].FirstOrDefault(),
            cancellationToken);

        return Ok(new
        {
            result.Processed,
            result.Message,
            result.PaymentId,
            result.SellerId
        });
    }
}
