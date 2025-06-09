using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, string RoleName) : ICommand;