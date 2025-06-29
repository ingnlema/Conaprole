using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conaprole.Orders.Infrastructure.Authentication;

internal sealed class KeycloakRealmSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KeycloakRealmSeederHostedService> _logger;

    public KeycloakRealmSeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<KeycloakRealmSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting Keycloak realm seeding hosted service...");
            
            using var scope = _serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IKeycloakRealmSeeder>();
            
            await seeder.SeedRealmAsync(cancellationToken);
            
            _logger.LogInformation("Keycloak realm seeding hosted service completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run Keycloak realm seeding hosted service");
            // Don't throw to prevent application startup failure
            // The application should still start even if seeding fails
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}