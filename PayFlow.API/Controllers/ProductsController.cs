using Microsoft.AspNetCore.Mvc;
using PayFlow.API.Services;
using PayFlow.API.DTOs.Requests;

namespace PayFlow.API.Controllers;

[ApiController]
[Route("api")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("sellers/{sellerId}/products")]
    public IActionResult CreateProduct(Guid sellerId, [FromBody] CreateProductRequest request)
    {
        var product = _productService.CreateProduct(sellerId, request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpGet("sellers/{sellerId}/products")]
    public IActionResult GetProductsBySeller(Guid sellerId, [FromQuery] string? category)
    {
        var products = string.IsNullOrEmpty(category)
            ? _productService.GetProductsBySeller(sellerId)
            : _productService.GetProductsByCategory(sellerId, category);
        return Ok(products);
    }

    [HttpGet("products/{id}")]
    public IActionResult GetProduct(Guid id)
    {
        var product = _productService.GetProduct(id);
        return Ok(product);
    }

    [HttpPut("products/{id}")]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = _productService.UpdateProduct(id, request);
        return Ok(product);
    }

    [HttpDelete("products/{id}")]
    public IActionResult DeleteProduct(Guid id)
    {
        _productService.DeleteProduct(id);
        return NoContent();
    }
}
