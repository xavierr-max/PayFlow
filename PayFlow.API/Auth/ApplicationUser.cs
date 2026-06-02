using Microsoft.AspNetCore.Identity;

namespace PayFlow.API.Auth;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid SellerId { get; set; }
    public bool IsPremium { get; set; }
}
