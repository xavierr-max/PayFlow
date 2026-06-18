namespace PayFlow.API.DTOs.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid SellerId { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
