using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Conaprole.Orders.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return;
        }

        // Check if user has permission claim in JWT token
        if (context.User.HasClaim("permissions", requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // Check for client roles in resource_access claim (Keycloak format)
        if (HasClientRole(context.User, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // Check for roles claim (alternative format)
        if (context.User.HasClaim("roles", requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // Fallback for Administrator role (legacy support)
        if (context.User.IsInRole("Administrator"))
        {
            context.Succeed(requirement);
            return;
        }

        // Fallback to database lookup for users without permission claims in JWT
        using var scope = _serviceProvider.CreateScope();

        var authorizationService = scope.ServiceProvider.GetRequiredService<Application.Abstractions.Authentication.IAuthorizationService>();

        var identityId = context.User.GetIdentityId();

        try
        {
            var permissions = await authorizationService.GetPermissionsForUserAsync(identityId);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
        catch
        {
            // User not found or other error - permission denied
            // No need to call context.Fail() as the default behavior is to deny
        }
    }

    private static bool HasClientRole(System.Security.Claims.ClaimsPrincipal user, string permission)
    {
        // Check resource_access claim for client roles (Keycloak format)
        var resourceAccessClaim = user.Claims.FirstOrDefault(c => c.Type == "resource_access");
        if (resourceAccessClaim != null)
        {
            try
            {
                var resourceAccess = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(resourceAccessClaim.Value);
                
                // Check orders-api client roles
                if (resourceAccess.TryGetValue("orders-api", out var clientData))
                {
                    var clientObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(clientData.ToString());
                    if (clientObj.TryGetValue("roles", out var rolesData))
                    {
                        var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(rolesData.ToString());
                        return roles.Contains(permission);
                    }
                }
            }
            catch
            {
                // Ignore JSON parsing errors
            }
        }

        return false;
    }
}