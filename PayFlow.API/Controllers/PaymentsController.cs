using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Auth;
using PayFlow.API.Exceptions;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
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
        EnsureSellerAccess(sellerId);
        var payment = _paymentService.CreatePayment(sellerId, request);
        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
    }

    [HttpGet("sellers/{sellerId}/payments")]
    public IActionResult GetPaymentsBySeller(Guid sellerId, [FromQuery] string? status)
    {
        EnsureSellerAccess(sellerId);
        var payments = string.IsNullOrEmpty(status)
            ? _paymentService.GetPaymentsBySeller(sellerId)
            : _paymentService.GetPaymentsByStatus(sellerId, status);
        return Ok(payments);
    }

    [HttpGet("payments/{id}")]
    public IActionResult GetPayment(Guid id)
    {
        var payment = _paymentService.GetPayment(id);
        EnsureSellerAccess(payment.SellerId);
        return Ok(payment);
    }

    [HttpPut("payments/{id}/mark-paid")]
    public IActionResult MarkAsPaid(Guid id, [FromBody] MarkPaymentPaidRequest request)
    {
        EnsurePaymentAccess(id);
        var payment = _paymentService.MarkAsPaid(id, request, User.GetUserId(), User.GetEmail());
        return Ok(payment);
    }

    [HttpPut("payments/{id}/unmark-paid")]
    public IActionResult UnmarkAsPaid(Guid id, [FromBody] UnmarkPaymentPaidRequest request)
    {
        EnsurePaymentAccess(id);
        var payment = _paymentService.UnmarkAsPaid(id, request, User.GetUserId(), User.GetEmail());
        return Ok(payment);
    }

    [HttpPut("payments/{id}/cancel")]
    public IActionResult CancelPayment(Guid id, [FromBody] CancelPaymentRequest request)
    {
        EnsurePaymentAccess(id);
        var payment = _paymentService.CancelPayment(id, request, User.GetUserId(), User.GetEmail());
        return Ok(payment);
    }

    [HttpPut("payments/{id}/dates")]
    public IActionResult UpdatePaymentDates(Guid id, [FromBody] UpdatePaymentDatesRequest request)
    {
        EnsurePaymentAccess(id);
        var payment = _paymentService.UpdatePaymentDates(id, request, User.GetUserId(), User.GetEmail());
        return Ok(payment);
    }

    [HttpGet("payments/{id}/transactions")]
    public IActionResult GetPaymentTransactions(Guid id)
    {
        EnsurePaymentAccess(id);
        var transactions = _paymentService.GetPaymentTransactions(id);
        return Ok(transactions);
    }

    [HttpPost("payments/{id}/sync")]
    [HttpPost("payments/{id}/sync-asaas-status")]
    public async Task<IActionResult> SyncPaymentStatus(Guid id, CancellationToken cancellationToken)
    {
        EnsurePaymentAccess(id);
        var payment = await _paymentService.SyncPaymentStatus(id, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/refund")]
    public async Task<IActionResult> RefundPayment(
        Guid id,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        EnsurePaymentAccess(id);
        var payment = await _paymentService.RefundPayment(id, request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/pix-whatsapp")]
    public async Task<IActionResult> CreatePixWhatsappMessage(Guid id, CancellationToken cancellationToken)
    {
        EnsurePaymentAccess(id);
        var payment = await _paymentService.CreatePixWhatsappMessage(id, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("payments/{id}/whatsapp")]
    public async Task<IActionResult> CreateWhatsappPaymentMessage(
        Guid id,
        [FromBody] CreatePaymentWhatsappMessageRequest request,
        CancellationToken cancellationToken)
    {
        EnsurePaymentAccess(id);
        var payment = await _paymentService.CreateWhatsappPaymentMessage(id, request, cancellationToken);
        return Ok(payment);
    }

    [HttpDelete("payments/{id}")]
    public IActionResult DeletePayment(Guid id)
    {
        EnsurePaymentAccess(id);
        _paymentService.DeletePayment(id);
        return NoContent();
    }

    private void EnsurePaymentAccess(Guid paymentId)
    {
        var payment = _paymentService.GetPayment(paymentId);
        EnsureSellerAccess(payment.SellerId);
    }

    private void EnsureSellerAccess(Guid sellerId)
    {
        if (!User.CanAccessSeller(sellerId))
            throw new UnauthorizedException("Você não tem acesso a este vendedor");
    }

}
