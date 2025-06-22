namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IAuthorizationService
{
    Task<HashSet<string>> GetPermissionsForUserAsync(string identityId);
    Task<UserRolesResponse> GetRolesForUserAsync(string identityId);
}