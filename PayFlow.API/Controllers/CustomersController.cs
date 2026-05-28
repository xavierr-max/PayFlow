using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api")]
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
        var customer = _customerService.CreateCustomer(sellerId, request);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpGet("sellers/{sellerId}/customers")]
    public IActionResult GetCustomersBySeller(Guid sellerId)
    {
        var customers = _customerService.GetCustomersBySeller(sellerId);
        return Ok(customers);
    }

    [HttpGet("customers/{id}")]
    public IActionResult GetCustomer(Guid id)
    {
        var customer = _customerService.GetCustomer(id);
        return Ok(customer);
    }

    [HttpPut("customers/{id}")]
    public IActionResult UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = _customerService.UpdateCustomer(id, request);
        return Ok(customer);
    }

    [HttpDelete("customers/{id}")]
    public IActionResult DeleteCustomer(Guid id)
    {
        _customerService.DeleteCustomer(id);
        return NoContent();
    }
}
