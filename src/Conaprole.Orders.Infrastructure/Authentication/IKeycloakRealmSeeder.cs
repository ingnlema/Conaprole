using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Infrastructure.Authentication;

public interface IKeycloakRealmSeeder
{
    Task SeedRealmAsync(CancellationToken cancellationToken = default);
    Task<string> GetTokenAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}