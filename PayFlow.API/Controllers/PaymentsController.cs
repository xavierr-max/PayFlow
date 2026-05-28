using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
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

    [HttpDelete("payments/{id}")]
    public IActionResult DeletePayment(Guid id)
    {
        _paymentService.DeletePayment(id);
        return NoContent();
    }
}
