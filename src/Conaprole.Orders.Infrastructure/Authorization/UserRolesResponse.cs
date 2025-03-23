using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Infrastructure.Authorization;

public class UserRolesResponse
{
    public Guid UserId { get; init; }

    public List<Role> Roles { get; init; } = [];
}