using System.Data;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Users.LoginUser;
using Dapper;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

public static class AuthorizationTestHelper
{
    public static async Task CreateUserWithPermissionAndSetAuthAsync(
        HttpClient httpClient, 
        ISqlConnectionFactory sqlConnectionFactory,
        string permission)
    {
        await CreateUserWithPermissionAndSetAuthAsync(httpClient, sqlConnectionFactory, permission, useExistingRole: false);
    }
    
    public static async Task CreateUserWithPermissionAndSetAuthAsync(
        HttpClient httpClient, 
        ISqlConnectionFactory sqlConnectionFactory,
        string permission,
        bool useExistingRole)
    {
        // Create user with specific permission through proper registration then update roles
        var email = $"authuser+{Guid.NewGuid():N}@test.com";
        var password = "TestPassword123";

        // Register user normally first
        var registerRequest = new RegisterUserRequest(email, "Auth", "User", password);
        var registerResponse = await httpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        using var connection = sqlConnectionFactory.CreateConnection();

        // Get the created user ID
        var userId = await connection.QuerySingleAsync<Guid>(@"
            SELECT id FROM users WHERE email = @Email", 
            new { Email = email });

        // Remove default roles
        await connection.ExecuteAsync(@"
            DELETE FROM role_user WHERE users_id = @UserId", 
            new { UserId = userId });

        // Create or find a role with the required permission
        int roleId;
        if (useExistingRole)
        {
            roleId = await GetExistingRoleWithPermissionAsync(connection, permission);
        }
        else
        {
            roleId = await CreateTestRoleWithPermissionAsync(connection, permission);
        }
        
        // Assign the specific role to user
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (users_id, roles_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = userId,
                RoleId = roleId
            });

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await httpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private static async Task<int> GetExistingRoleWithPermissionAsync(IDbConnection connection, string permission)
    {
        // Find an existing role that has the required permission
        var roleId = await connection.QuerySingleAsync<int>(@"
            SELECT rp.role_id 
            FROM role_permissions rp
            INNER JOIN permissions p ON rp.permission_id = p.id
            WHERE p.name = @Permission
            ORDER BY rp.role_id
            LIMIT 1",
            new { Permission = permission });

        return roleId;
    }

    private static async Task<int> CreateTestRoleWithPermissionAsync(IDbConnection connection, string permission)
    {
        // Create a unique test role name
        var roleName = $"TestRole_{permission.Replace(":", "_")}_{Guid.NewGuid():N}";
        
        // Insert the test role
        var roleId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO roles (name) VALUES (@RoleName) RETURNING id",
            new { RoleName = roleName });

        // Get the permission ID
        var permissionId = await connection.QuerySingleAsync<int>(@"
            SELECT id FROM permissions WHERE name = @Permission",
            new { Permission = permission });

        // Assign the permission to the test role
        await connection.ExecuteAsync(@"
            INSERT INTO role_permissions (role_id, permission_id) 
            VALUES (@RoleId, @PermissionId)",
            new { RoleId = roleId, PermissionId = permissionId });

        return roleId;
    }
}