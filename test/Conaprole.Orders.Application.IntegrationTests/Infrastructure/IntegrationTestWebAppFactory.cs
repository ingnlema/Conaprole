using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Infrastructure;
using Conaprole.Orders.Infrastructure.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Keycloak;
using Conaprole.Orders.Infrastructure.Data;

namespace Conaprole.Orders.Application.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly KeycloakContainer _keycloakContainer;
    
    public TestUserContext TestUserContext { get; private set; } = null!;
    
    public IntegrationTestWebAppFactory()
    {
        // Set environment variable for Docker compatibility in CI environments
        Environment.SetEnvironmentVariable("DOCKER_DEFAULT_PLATFORM", "linux/amd64");
        
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine") // Use specific stable version for better performance
            .WithDatabase("conaprole.orders")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithStartupCallback(async (container, cancellationToken) =>
            {
                // Wait for PostgreSQL to be ready before proceeding
                await Task.Delay(2000, cancellationToken);
            })
            .Build();
        
        _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:21.1.1") // Use specific version for stability
            .WithResourceMapping(
                new FileInfo(".files/conaprole-realm-export.json"),
                new FileInfo("/opt/keycloak/data/import/realm.json"))
            .WithCommand("--import-realm")
            .WithStartupCallback(async (container, cancellationToken) =>
            {
                // Give Keycloak more time to start and import realm
                await Task.Delay(5000, cancellationToken);
            })
            .Build();
    }
    
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention());
            
            services.RemoveAll(typeof(ISqlConnectionFactory));
            
            services.AddSingleton<ISqlConnectionFactory>(_ => 
                new SqlConnectionFactory(_dbContainer.GetConnectionString()));
            
            // Replace IUserContext with TestUserContext for testing
            TestUserContext = new TestUserContext();
            services.RemoveAll(typeof(IUserContext));
            services.AddSingleton<IUserContext>(TestUserContext);
            
            var keycloackAddress = _keycloakContainer.GetBaseAddress();
            
            var keycloakAddress = _keycloakContainer.GetBaseAddress();
            if (!keycloakAddress.EndsWith("/"))
            {
                keycloakAddress += "/";
            }
            
            services.Configure<KeycloakOptions>(o =>
            {
                o.AdminUrl = $"{keycloakAddress}admin/realms/Conaprole/";
                o.TokenUrl = $"{keycloakAddress}realms/Conaprole/protocol/openid-connect/token";
            });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Start containers with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            
            // Start PostgreSQL first (usually more reliable)
            await _dbContainer.StartAsync(cts.Token);
            
            // Try to start Keycloak with retry logic due to Docker environment issues
            var keycloakStarted = false;
            var retryCount = 0;
            const int maxRetries = 3;
            
            while (!keycloakStarted && retryCount < maxRetries)
            {
                try
                {
                    await _keycloakContainer.StartAsync(cts.Token);
                    keycloakStarted = true;
                }
                catch (Exception ex) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    System.Diagnostics.Debug.WriteLine($"Keycloak startup attempt {retryCount} failed: {ex.Message}. Retrying...");
                    
                    // Clean up the failed container
                    try
                    {
                        await _keycloakContainer.StopAsync(CancellationToken.None);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                    
                    // Wait before retry
                    await Task.Delay(2000, cts.Token);
                }
            }
            
            if (!keycloakStarted)
            {
                throw new InvalidOperationException("Failed to start Keycloak container after multiple attempts. This may be due to Docker environment restrictions or resource limitations.");
            }
            
            // Additional wait for services to be fully ready
            await Task.Delay(2000, cts.Token);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Timed out waiting for test containers to start. This may be due to network restrictions or slow container initialization.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize test containers: {ex.Message}", ex);
        }
    }

    public new async Task DisposeAsync()
    {
        try
        {
            // Stop containers with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            
            await _dbContainer.StopAsync(cts.Token);
            await _keycloakContainer.StopAsync(cts.Token);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - disposal should be silent
            System.Diagnostics.Debug.WriteLine($"Warning: Error during container cleanup: {ex.Message}");
        }
    }
}