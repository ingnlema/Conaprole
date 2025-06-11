using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Users.LoginUser;

namespace Conaprole.Orders.Application.Users.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AccessTokenResponse>;