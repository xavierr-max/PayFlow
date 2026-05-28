namespace PayFlow.API.DTOs.Requests;

public class CreateCustomerRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
}
