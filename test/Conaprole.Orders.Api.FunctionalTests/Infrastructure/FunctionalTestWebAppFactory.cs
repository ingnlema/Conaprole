using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.FunctionalTests.Users;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Infrastructure;
using Conaprole.Orders.Infrastructure.Authentication;
using Conaprole.Orders.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;


namespace Conaprole.Orders.Api.FunctionalTests.Infrastructure;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly KeycloakContainer _keycloakContainer;
    
    public FunctionalTestWebAppFactory()
    {
        Environment.SetEnvironmentVariable("DOCKER_DEFAULT_PLATFORM", "linux/arm64");
        
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine") 
            .WithDatabase("conaprole.orders")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        
        _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:21.1.1") 
            .WithResourceMapping(
                new FileInfo(".files/conaprole-realm-export.json"),
                new FileInfo("/opt/keycloak/data/import/realm.json"))
            .WithCommand("--import-realm")
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

            services.Configure<AuthenticationOptions>(o =>
            {
                o.Issuer = $"{keycloakAddress}realms/Conaprole/";
                o.MetadataUrl = $"{keycloakAddress}realms/Conaprole/.well-known/openid-configuration";
            });

        });

        builder.ConfigureTestServices(services =>
        {
            // Remove existing seeding hosted service to prevent conflicts in tests
            var hostedServiceDescriptor = services.FirstOrDefault(
                d => d.ImplementationType == typeof(KeycloakRealmSeederHostedService));
            if (hostedServiceDescriptor != null)
            {
                services.Remove(hostedServiceDescriptor);
            }
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        
        // Espera a que Keycloak esté listo
        await WaitForKeycloakReadyAsync(CancellationToken.None);
        
        // Seed Keycloak realm for testing
        await SeedKeycloakRealmAsync();
        
        await InitializeTestUserAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _keycloakContainer.StopAsync();
    }
    
    private async Task WaitForKeycloakReadyAsync(CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient();
        
        var keycloakUrl = _keycloakContainer.GetBaseAddress();
        if (!keycloakUrl.EndsWith("/"))
        {
            keycloakUrl += "/";
        }
        var endpoint = $"{keycloakUrl}realms/Conaprole/.well-known/openid-configuration";
        
        var timeout = TimeSpan.FromSeconds(120);
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var response = await httpClient.GetAsync(endpoint, cancellationToken);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Attempt to reach Keycloak failed: {ex.Message}");
            }

            await Task.Delay(1000, cancellationToken);
        }

        throw new TimeoutException($"Keycloak did not become ready at {endpoint} after {timeout.TotalSeconds} seconds.");
    }

    private async Task SeedKeycloakRealmAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IKeycloakRealmSeeder>();
            await seeder.SeedRealmAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to seed Keycloak realm: {ex.Message}");
            // Don't throw - allow tests to continue with basic setup
        }
    }

    private async Task InitializeTestUserAsync()
    {
        var httpClient = CreateClient();
        await httpClient.PostAsJsonAsync("api/users/register", UserData.RegisterTestUserRequest);
    }
}