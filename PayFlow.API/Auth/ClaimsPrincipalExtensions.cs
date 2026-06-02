using System.Security.Claims;

namespace PayFlow.API.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : null;
    }

    public static Guid? GetSellerId(this ClaimsPrincipal user)
    {
        var sellerId = user.FindFirstValue("seller_id");
        return Guid.TryParse(sellerId, out var parsed) ? parsed : null;
    }

    public static bool CanAccessSeller(this ClaimsPrincipal user, Guid sellerId)
    {
        return user.IsInRole("Admin") || user.GetSellerId() == sellerId;
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email");
    }
}
