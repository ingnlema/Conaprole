namespace Conaprole.Orders.Application.Abstractions.Authentication;

public sealed record TokenResult(string AccessToken, string RefreshToken);