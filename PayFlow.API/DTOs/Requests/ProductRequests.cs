namespace PayFlow.API.DTOs.Requests;

public class CreateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string CategoryId { get; set; }
    public string ImageUrl { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string CategoryId { get; set; }
    public string ImageUrl { get; set; }
}
