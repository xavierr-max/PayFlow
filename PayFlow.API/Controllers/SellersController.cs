using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api/sellers")]
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
        var seller = _sellerService.GetSeller(id);
        return Ok(seller);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateSeller(Guid id, [FromBody] UpdateSellerRequest request)
    {
        var seller = _sellerService.UpdateSeller(id, request);
        return Ok(seller);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSeller(Guid id)
    {
        _sellerService.DeleteSeller(id);
        return NoContent();
    }
}
