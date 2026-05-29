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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost("sellers/{sellerId}/customers")]
    public IActionResult CreateCustomer(Guid sellerId, [FromBody] CreateCustomerRequest request)
    {
        EnsureSellerAccess(sellerId);
        var customer = _customerService.CreateCustomer(sellerId, request);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpGet("sellers/{sellerId}/customers")]
    public IActionResult GetCustomersBySeller(Guid sellerId)
    {
        EnsureSellerAccess(sellerId);
        var customers = _customerService.GetCustomersBySeller(sellerId);
        return Ok(customers);
    }

    [HttpGet("customers/{id}")]
    public IActionResult GetCustomer(Guid id)
    {
        var customer = _customerService.GetCustomer(id);
        EnsureSellerAccess(customer.SellerId);
        return Ok(customer);
    }

    [HttpPut("customers/{id}")]
    public IActionResult UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var existing = _customerService.GetCustomer(id);
        EnsureSellerAccess(existing.SellerId);
        var customer = _customerService.UpdateCustomer(id, request);
        return Ok(customer);
    }

    [HttpDelete("customers/{id}")]
    public IActionResult DeleteCustomer(Guid id)
    {
        var existing = _customerService.GetCustomer(id);
        EnsureSellerAccess(existing.SellerId);
        _customerService.DeleteCustomer(id);
        return NoContent();
    }

    private void EnsureSellerAccess(Guid sellerId)
    {
        if (!User.CanAccessSeller(sellerId))
            throw new UnauthorizedException("Você não tem acesso a este vendedor");
    }
}
