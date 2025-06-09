using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, string RoleName) : ICommand;