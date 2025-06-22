using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Abstractions.Authentication;

public class UserRolesResponse
{
    public Guid UserId { get; init; }

    public List<Role> Roles { get; init; } = [];
}