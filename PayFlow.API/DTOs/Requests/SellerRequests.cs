namespace PayFlow.API.DTOs.Requests;

public class CreateSellerRequest
{
    public string Name { get; set; }
    public string StoreName { get; set; }
    public string PixKey { get; set; }
}

public class UpdateSellerRequest
{
    public string Name { get; set; }
    public string StoreName { get; set; }
    public string PixKey { get; set; }
}
