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

namespace Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private readonly KeycloakContainer _keycloakContainer;
    public TestUserContext TestUserContext { get; } = new();
    
    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("conaprole.orders")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        
        _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:latest") // o la versión que quieras
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
            
            // Replace IUserContext with test implementation
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
        await _dbContainer.StartAsync();
        await _keycloakContainer.StartAsync();
        
        // Apply migrations to the test database
        using var scope = Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _keycloakContainer.StopAsync();
    }
}