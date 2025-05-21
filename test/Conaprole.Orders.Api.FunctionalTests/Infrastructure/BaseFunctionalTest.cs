using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Users;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Conaprole.Orders.Api.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly ISqlConnectionFactory SqlConnectionFactory;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        SqlConnectionFactory = factory.Services.GetRequiredService<ISqlConnectionFactory>();
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
    
    protected async Task<Guid> CreatePointOfSaleAsync(string phoneNumber = "+59891234567")
    {
        var id = Guid.NewGuid();

        var sql = @"
        INSERT INTO point_of_sale (id, phone_number, address, is_active, created_at)
        VALUES (@Id, @PhoneNumber, @Address, true, now());";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            PhoneNumber = phoneNumber,
            Address = "POS Test Address"
        });

        return id;
    }
    
    protected async Task<Guid> CreateDistributorAsync(string phoneNumber = "+59899887766")
    {
        var id = Guid.NewGuid();

        const string sql = @"
        INSERT INTO distributor (id, phone_number, name, address, supported_categories)
        VALUES (@Id, @PhoneNumber, @Name, @Address, @Categories);";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            PhoneNumber = phoneNumber,
            Name = "Distribuidor Test",
            Address = "Calle Falsa 123",
            Categories = new[] { (int)Category.LACTEOS }
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
            Category = (int)category
        });

        return id;
    }
    
    
}