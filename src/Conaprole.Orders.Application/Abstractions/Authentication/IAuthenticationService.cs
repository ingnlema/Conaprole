using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<string> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        string identityId,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string identityId,
        CancellationToken cancellationToken = default);
}