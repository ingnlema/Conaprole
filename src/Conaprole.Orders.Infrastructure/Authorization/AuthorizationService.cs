using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Authorization;

internal sealed class AuthorizationService : IAuthorizationService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthorizationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserRolesResponse> GetRolesForUserAsync(string identityId)
    {
        var roles = await _dbContext.Set<User>()
            .Where(u => u.IdentityId == identityId)
            .Select(u => new UserRolesResponse
            {
                UserId = u.Id,
                Roles = u.Roles.ToList()
            })
            .FirstOrDefaultAsync();

        if (roles == null)
        {
            throw new UnauthorizedAccessException($"User with identity ID '{identityId}' not found");
        }

        return roles;
    }

    public async Task<HashSet<string>> GetPermissionsForUserAsync(string identityId)
    {
        var permissions = await _dbContext.Set<User>()
            .Where(u => u.IdentityId == identityId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.Permissions)
            .Select(p => p.Name)
            .ToListAsync();

        return permissions.ToHashSet();
    }
}