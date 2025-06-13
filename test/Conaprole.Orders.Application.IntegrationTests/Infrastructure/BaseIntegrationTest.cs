using Conaprole.Orders.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Conaprole.Orders.Application.Abstractions.Data;
using Dapper;

namespace Conaprole.Orders.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected readonly ApplicationDbContext DbContext;
    protected readonly ISqlConnectionFactory SqlConnectionFactory;
    protected readonly TestUserContext TestUserContext;
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        SqlConnectionFactory = _scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
        TestUserContext = factory.TestUserContext;
    }
    
    public async Task InitializeAsync()
    {
        await CleanDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
    
    protected async Task CleanDatabaseAsync()
    {
        using var connection = SqlConnectionFactory.CreateConnection();

        await connection.ExecuteAsync("DELETE FROM point_of_sale_distributor");
        await connection.ExecuteAsync("DELETE FROM order_lines");
        await connection.ExecuteAsync("DELETE FROM orders");
        await connection.ExecuteAsync("DELETE FROM products");
        await connection.ExecuteAsync("DELETE FROM point_of_sale");
        // Clean role_user junction table before deleting users to avoid FK constraint violations
        await connection.ExecuteAsync("DELETE FROM role_user");
        // Delete all users, not just those with distributors
        await connection.ExecuteAsync("DELETE FROM users");
        await connection.ExecuteAsync("DELETE FROM distributor");
    }
}