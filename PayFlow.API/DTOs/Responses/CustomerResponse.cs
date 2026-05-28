namespace PayFlow.API.DTOs.Responses;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
    public Guid SellerId { get; set; }
}
