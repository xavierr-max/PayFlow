using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Auth;
using PayFlow.API.Exceptions;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/sellers")]
[Authorize]
public class SellersController : ControllerBase
{
    private readonly ISellerService _sellerService;

    public SellersController(ISellerService sellerService)
    {
        _sellerService = sellerService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSeller([FromBody] CreateSellerRequest request, CancellationToken cancellationToken)
    {
        var seller = await _sellerService.CreateSellerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSeller), new { id = seller.Id }, seller);
    }

    [HttpGet("{id}")]
    public IActionResult GetSeller(Guid id)
    {
        EnsureSellerAccess(id);
        var seller = _sellerService.GetSeller(id);
        return Ok(seller);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSeller(
        Guid id,
        [FromBody] UpdateSellerRequest request,
        CancellationToken cancellationToken)
    {
        EnsureSellerAccess(id);
        var seller = await _sellerService.UpdateSellerAsync(id, request, cancellationToken);
        return Ok(seller);
    }

    [HttpPost("{id}/asaas/subaccount")]
    public async Task<IActionResult> CreateAsaasSubaccount(Guid id, CancellationToken cancellationToken)
    {
        EnsureSellerAccess(id);
        var account = await _sellerService.CreateAsaasSubaccountAsync(id, cancellationToken);
        return Ok(account);
    }

    [HttpPut("{id}/asaas/subaccount-profile")]
    public async Task<IActionResult> UpdateAsaasSubaccountProfile(
        Guid id,
        [FromBody] UpdateSellerAsaasSubaccountRequest request,
        CancellationToken cancellationToken)
    {
        EnsureSellerAccess(id);
        var seller = await _sellerService.UpdateSellerAsaasSubaccountProfileAsync(id, request, cancellationToken);
        return Ok(seller);
    }

    [HttpGet("{id}/asaas/status")]
    public IActionResult GetAsaasAccountStatus(Guid id)
    {
        EnsureSellerAccess(id);
        var status = _sellerService.GetAsaasAccountStatus(id);
        return Ok(status);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSeller(Guid id)
    {
        EnsureSellerAccess(id);
        _sellerService.DeleteSeller(id);
        return NoContent();
    }

    private void EnsureSellerAccess(Guid sellerId)
    {
        if (!User.CanAccessSeller(sellerId))
            throw new UnauthorizedException("Você não tem acesso a este vendedor");
    }
}
