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

        // Use database as the single source of truth for authorization
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
}