using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;
using Stripe;
using Stripe.Checkout;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly StripeOptions _stripeOptions;

    public PaymentsController(IPaymentService paymentService, IOptions<StripeOptions> stripeOptions)
    {
        _paymentService = paymentService;
        _stripeOptions = stripeOptions.Value;
    }

    [HttpPost("sellers/{sellerId}/payments")]
    public IActionResult CreatePayment(Guid sellerId, [FromBody] CreatePaymentRequest request)
    {
        var payment = _paymentService.CreatePayment(sellerId, request);
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    [HttpGet("sellers/{sellerId}/payments")]
    public IActionResult GetPaymentsBySeller(Guid sellerId, [FromQuery] string? status)
    {
        var payments = string.IsNullOrEmpty(status)
            ? _paymentService.GetPaymentsBySeller(sellerId)
            : _paymentService.GetPaymentsByStatus(sellerId, status);
        return Ok(payments);
    }

    [HttpGet("payments/{id}")]
    public IActionResult GetPayment(Guid id)
    {
        var payment = _paymentService.GetPayment(id);
        return Ok(payment);
    }

    [HttpPut("payments/{id}/mark-paid")]
    public IActionResult MarkAsPaid(Guid id, [FromBody] MarkPaymentPaidRequest request)
    {
        var payment = _paymentService.MarkAsPaid(id, request);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/pix-whatsapp")]
    public async Task<IActionResult> CreatePixWhatsappMessage(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreatePixWhatsappMessage(id, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/whatsapp")]
    public async Task<IActionResult> CreateWhatsappPaymentMessage(
        Guid id,
        [FromBody] CreatePaymentWhatsappMessageRequest? request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreateWhatsappPaymentMessage(id, request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/sync-stripe-status")]
    public async Task<IActionResult> SyncStripePaymentStatus(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.RefreshStripePaymentStatus(id, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("stripe/webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);

        Event stripeEvent;
        try
        {
            stripeEvent = string.IsNullOrWhiteSpace(_stripeOptions.WebhookSecret)
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _stripeOptions.WebhookSecret);
        }
        catch (StripeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (stripeEvent.Data.Object is PaymentIntent paymentIntent &&
            paymentIntent.Metadata.TryGetValue("payflow_payment_id", out var paymentIdValue) &&
            Guid.TryParse(paymentIdValue, out var paymentId))
        {
            _paymentService.SyncStripePaymentIntentStatus(paymentId, paymentIntent.Id, paymentIntent.Status);
        }
        else if (stripeEvent.Data.Object is Session session &&
                 session.Metadata.TryGetValue("payflow_payment_id", out var checkoutPaymentIdValue) &&
                 Guid.TryParse(checkoutPaymentIdValue, out var checkoutPaymentId))
        {
            _paymentService.SyncStripeCheckoutSessionStatus(
                checkoutPaymentId,
                session.Id,
                session.PaymentIntentId,
                session.PaymentStatus);
        }

        return Ok();
    }

    [HttpDelete("payments/{id}")]
    public IActionResult DeletePayment(Guid id)
    {
        _paymentService.DeletePayment(id);
        return NoContent();
    }
}
