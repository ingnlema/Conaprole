using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;