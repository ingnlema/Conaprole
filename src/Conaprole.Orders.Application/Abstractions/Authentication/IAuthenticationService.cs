using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<string> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);
}