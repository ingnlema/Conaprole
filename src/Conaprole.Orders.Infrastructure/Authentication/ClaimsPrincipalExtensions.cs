using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Conaprole.Orders.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        // First try to get the internal user ID from our custom claim
        var internalUserId = principal?.FindFirstValue("internal_user_id");
        if (!string.IsNullOrEmpty(internalUserId) && Guid.TryParse(internalUserId, out var parsedInternalUserId))
        {
            return parsedInternalUserId;
        }

        // Fallback to try parsing sub claim as Guid (legacy support)
        var userId = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedUserId))
        {
            return parsedUserId;
        }

        throw new ApplicationException("User identifier is unavailable");
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
               principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
               throw new ApplicationException("User identity is unavailable");
    }
}
