using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Users;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Conaprole.Orders.Api.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>, IAsyncLifetime
{
    protected readonly HttpClient HttpClient;
    protected readonly ISqlConnectionFactory SqlConnectionFactory;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        SqlConnectionFactory = factory.Services.GetRequiredService<ISqlConnectionFactory>();
    }
    
    public async Task InitializeAsync()
    {
        await CleanDatabaseAsync();
        await RegisterTestUserAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task RegisterTestUserAsync()
    {
        // Register the test user that will be used for authentication
        var registerResponse = await HttpClient.PostAsJsonAsync(
            "/api/users/register", 
            UserData.RegisterTestUserRequest);
        
        // If user already exists, that's fine - we can use the existing user
        if (registerResponse.StatusCode == HttpStatusCode.Conflict)
        {
            return; // User already exists, continue
        }
        
        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register test user: {error}");
        }
    }
    
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

    protected async Task<string> GetAccessToken()
    {
        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "/api/users/login",
            new LogInUserRequest(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));
        
        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        return accessTokenResponse!.AccessToken;
    }

    protected async Task SetAuthorizationHeaderAsync()
    {
        var token = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    protected async Task<Guid> CreatePointOfSaleAsync(string phoneNumber = "+59891234567")
    {
        var id = Guid.NewGuid();

        var sql = @"
        INSERT INTO point_of_sale (id, phone_number, name, address, is_active, created_at)
        VALUES (@Id, @PhoneNumber, @Name, @Address, true, now());";

        // Create a properly formatted address string that matches Address.ToString() output
        var testAddress = new Address("Montevideo", "Avenida Test 123", "11000");
        var addressString = testAddress.ToString();

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Name = "POS de Prueba",
            PhoneNumber = phoneNumber,
            Address = addressString
        });

        return id;
    }
    
    protected async Task<Guid> CreateInactivePointOfSaleAsync(string phoneNumber = "+59891234568")
    {
        var id = Guid.NewGuid();

        var sql = @"
        INSERT INTO point_of_sale (id, phone_number, name, address, is_active, created_at)
        VALUES (@Id, @PhoneNumber, @Name, @Address, false, now());";

        // Create a properly formatted address string that matches Address.ToString() output
        var testAddress = new Address("Punta del Este", "Calle Test 456", "20000");
        var addressString = testAddress.ToString();

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Name = "POS Inactivo de Prueba",
            PhoneNumber = phoneNumber,
            Address = addressString
        });

        return id;
    }
    
    protected async Task<Guid> CreateDistributorAsync(string phoneNumber = "+59899887766")
    {
        var id = Guid.NewGuid();

        const string sql = @"
        INSERT INTO distributor (id, phone_number, name, address, supported_categories, created_at)
        VALUES (@Id, @PhoneNumber, @Name, @Address, @Categories, now());";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            PhoneNumber = phoneNumber,
            Name = "Distribuidor Test",
            Address = "Calle Falsa 123",
            Categories = string.Join(",", new[] { Category.LACTEOS.ToString() })
        });

        return id;
    }
    
    protected async Task<Guid> CreateProductAsync(string externalId = "TEST-001", Category category = Category.LACTEOS)
    {
        var id = Guid.NewGuid();

        const string sql = @"
        INSERT INTO products (id, external_product_id, name, unit_price_amount, unit_price_currency, description, last_updated, category)
        VALUES (@Id, @ExternalId, @Name, @Amount, @Currency, @Description, now(), @Category);";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            ExternalId = externalId,
            Name = "Producto Test",
            Amount = 100.0m,
            Currency = "UYU",
            Description = "Producto de prueba",
            Category = (int)category
        });

        return id;
    }
    
    protected async Task<Guid> AssignDistributorToPointOfSaleAsync(Guid posId, Guid distributorId, Category category)
    {
        var id = Guid.NewGuid();

        const string sql = @"
        INSERT INTO point_of_sale_distributor (id, point_of_sale_id, distributor_id, category, assigned_at)
        VALUES (@Id, @PointOfSaleId, @DistributorId, @Category, now());";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            PointOfSaleId = posId,
            DistributorId = distributorId,
            Category = category.ToString()
        });

        return id;
    }
    
    protected async Task<bool> IsDistributorAssignedAsync(Guid posId, Guid distributorId, Category category)
    {
        const string sql = @"
        SELECT COUNT(1) FROM point_of_sale_distributor 
        WHERE point_of_sale_id = @PointOfSaleId 
        AND distributor_id = @DistributorId 
        AND category = @Category";

        using var connection = SqlConnectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new
        {
            PointOfSaleId = posId,
            DistributorId = distributorId,
            Category = category.ToString()
        });

        return count > 0;
    }
    
    
}