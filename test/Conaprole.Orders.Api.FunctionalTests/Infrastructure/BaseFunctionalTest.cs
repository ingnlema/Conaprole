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
        // First check if user already exists in local database
        using var connection = SqlConnectionFactory.CreateConnection();
        var existingUser = await connection.QuerySingleOrDefaultAsync<dynamic>(@"
            SELECT id, email FROM users WHERE email = @Email",
            new { Email = UserData.RegisterTestUserRequest.Email });

        if (existingUser != null)
        {
            // User already exists in database, no need to register
            return;
        }

        // Register the test user that will be used for authentication
        var registerResponse = await HttpClient.PostAsJsonAsync(
            "/api/users/register", 
            UserData.RegisterTestUserRequest);
        
        // If user already exists in Keycloak but not in our DB, we need to handle this
        if (registerResponse.StatusCode == HttpStatusCode.Conflict)
        {
            // User exists in Keycloak but not in our local DB (due to cleanup)
            // We need to manually create the user in the local database with proper roles
            await CreateTestUserManuallyAsync();
            return;
        }
        
        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register test user: {error}");
        }
    }

    private async Task CreateTestUserManuallyAsync()
    {
        // Since the user exists in Keycloak but not in our DB, let's try to login
        // to get the real IdentityId from the JWT token, then create the DB user with that ID
        
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", new LogInUserRequest(
            UserData.RegisterTestUserRequest.Email,
            UserData.RegisterTestUserRequest.Password));

        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Could not login to existing Keycloak user: {loginResponse.StatusCode}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        var accessToken = loginResult!.AccessToken;
        
        // Extract the subject (sub) claim from the JWT token - this is the IdentityId
        var identityId = ExtractSubjectFromJwt(accessToken);
        
        using var connection = SqlConnectionFactory.CreateConnection();
        var userId = Guid.NewGuid();
        
        // Insert user with the real IdentityId from Keycloak
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, identity_id, first_name, last_name, email, created_at)
            VALUES (@Id, @IdentityId, @FirstName, @LastName, @Email, now())",
            new
            {
                Id = userId,
                IdentityId = identityId,
                FirstName = UserData.RegisterTestUserRequest.FirstName,
                LastName = UserData.RegisterTestUserRequest.LastName,
                Email = UserData.RegisterTestUserRequest.Email
            });

        // Assign the Registered role (ID = 1)
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (users_id, roles_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = userId,
                RoleId = 1 // Registered role ID
            });
    }

    private static string ExtractSubjectFromJwt(string jwt)
    {
        // Simple JWT parsing to extract the subject claim
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid JWT format");
        }

        var payload = parts[1];
        // Add padding if needed
        while (payload.Length % 4 != 0)
        {
            payload += "=";
        }

        var payloadBytes = Convert.FromBase64String(payload);
        var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
        
        // Parse the JSON to extract the "sub" claim
        using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
        if (doc.RootElement.TryGetProperty("sub", out var subElement))
        {
            return subElement.GetString()!;
        }

        throw new InvalidOperationException("Could not extract subject from JWT token");
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

    protected async Task<string> CreateAndGetAdminAccessTokenAsync()
    {
        // Generate unique admin user credentials
        var adminEmail = $"admin+{Guid.NewGuid():N}@test.com";
        var adminPassword = "admin123";

        // Try to register admin user normally first
        var registerRequest = new RegisterUserRequest(adminEmail, "Admin", "User", adminPassword);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        
        if (registerResponse.StatusCode == HttpStatusCode.Conflict)
        {
            // Handle Keycloak conflict like we do for test users
            await CreateAdminUserManuallyAsync(adminEmail, adminPassword);
        }
        else if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register admin user: {error}");
        }
        else
        {
            // User was created successfully, now assign additional admin roles
            await AssignAdminRolesToUserAsync(adminEmail);
        }

        // Login to get access token
        var loginRequest = new LogInUserRequest(adminEmail, adminPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        
        if (!loginResponse.IsSuccessStatusCode)
        {
            var error = await loginResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to login admin user: {error}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return loginResult!.AccessToken;
    }

    private async Task CreateAdminUserManuallyAsync(string adminEmail, string adminPassword)
    {
        // Login to get the real IdentityId from Keycloak
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", new LogInUserRequest(
            adminEmail, adminPassword));

        if (!loginResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Could not login to existing Keycloak admin user: {loginResponse.StatusCode}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        var identityId = ExtractSubjectFromJwt(loginResult!.AccessToken);
        
        using var connection = SqlConnectionFactory.CreateConnection();
        var adminUserId = Guid.NewGuid();
        
        // Insert admin user with real IdentityId
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, identity_id, first_name, last_name, email, created_at)
            VALUES (@Id, @IdentityId, @FirstName, @LastName, @Email, now())",
            new
            {
                Id = adminUserId,
                IdentityId = identityId,
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail
            });

        // Assign Administrator role
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (users_id, roles_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = adminUserId,
                RoleId = 3 // Administrator role ID
            });

        // Also assign Registered role (default)
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (users_id, roles_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = adminUserId,
                RoleId = 1 // Registered role ID
            });
    }

    private async Task AssignAdminRolesToUserAsync(string adminEmail)
    {
        using var connection = SqlConnectionFactory.CreateConnection();
        
        // Get the user ID
        var userId = await connection.QuerySingleAsync<Guid>(@"
            SELECT id FROM users WHERE email = @Email", 
            new { Email = adminEmail });

        // Check if user already has Administrator role
        var hasAdminRole = await connection.QuerySingleAsync<bool>(@"
            SELECT COUNT(1) > 0 FROM role_user 
            WHERE users_id = @UserId AND roles_id = @RoleId",
            new { UserId = userId, RoleId = 3 });

        if (!hasAdminRole)
        {
            // Assign Administrator role
            await connection.ExecuteAsync(@"
                INSERT INTO role_user (users_id, roles_id)
                VALUES (@UserId, @RoleId)",
                new
                {
                    UserId = userId,
                    RoleId = 3 // Administrator role ID
                });
        }
    }

    protected async Task SetAdminAuthorizationHeaderAsync()
    {
        var token = await CreateAndGetAdminAccessTokenAsync();
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