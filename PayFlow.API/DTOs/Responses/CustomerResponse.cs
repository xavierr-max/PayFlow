namespace PayFlow.API.DTOs.Responses;

public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Email { get; set; }
    public string? AsaasCustomerId { get; set; }
    public Guid SellerId { get; set; }
}
