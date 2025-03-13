using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IJwtService
{
    Task<Result<string>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}