using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IJwtService
{
    Task<Result<TokenResult>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResult>> GetAccessTokenFromRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}