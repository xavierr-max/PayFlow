namespace PayFlow.API.DTOs.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Guid SellerId { get; set; }
    public string CategoryId { get; set; }
    public string ImageUrl { get; set; }
}

