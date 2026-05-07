using System.Security.Claims;

namespace FoodShop.API.Helpers;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return int.Parse(claim);
    }

    public static string GetUserRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value ?? "User";
}
