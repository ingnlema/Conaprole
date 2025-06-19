using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string NewPassword) : ICommand;