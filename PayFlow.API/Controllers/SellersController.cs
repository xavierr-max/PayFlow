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
    public IActionResult CreateSeller([FromBody] CreateSellerRequest request)
    {
        var seller = _sellerService.CreateSeller(request);
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
    public IActionResult UpdateSeller(Guid id, [FromBody] UpdateSellerRequest request)
    {
        EnsureSellerAccess(id);
        var seller = _sellerService.UpdateSeller(id, request);
        return Ok(seller);
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
