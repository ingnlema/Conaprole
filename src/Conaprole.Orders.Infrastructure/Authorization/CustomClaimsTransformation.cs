using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Conaprole.Orders.Infrastructure.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Conaprole.Orders.Application.Abstractions.Authentication;

namespace Conaprole.Orders.Infrastructure.Authorization;

internal sealed class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;

    public CustomClaimsTransformation(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true } ||
            principal.HasClaim(claim => claim.Type == ClaimTypes.Role) &&
            principal.HasClaim(claim => claim.Type == JwtRegisteredClaimNames.Sub))
        {
            return principal;
        }

        using var scope = _serviceProvider.CreateScope();

        var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        var identityId = principal.GetIdentityId();

        try
        {
            var userRoles = await authorizationService.GetRolesForUserAsync(identityId);

            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userRoles.UserId.ToString()));

            foreach (var role in userRoles.Roles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            principal.AddIdentity(claimsIdentity);
        }
        catch (UnauthorizedAccessException)
        {
            // User was deleted or doesn't exist - don't enrich claims
            // The authentication will fail later with proper error handling
        }

        return principal;
    }
}