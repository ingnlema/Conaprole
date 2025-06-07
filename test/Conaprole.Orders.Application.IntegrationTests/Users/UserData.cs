using Conaprole.Orders.Application.Abstractions.Data;
using Dapper;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

/// <summary>
/// Encapsula datos y creación de un usuario para tests de integración.
/// </summary>
public static class UserData
{
    public const string TestEmail = "testuser@integration.com";
    public const string TestFirstName = "Test";
    public const string TestLastName = "User";
    public const string TestIdentityId = "test-identity-12345";
    public static readonly Guid TestUserId = new("12345678-1234-1234-1234-123456789012");

    /// <summary>
    /// Crea el usuario directamente en la base de datos y asigna el rol "Registered".
    /// </summary>
    public static async Task<Guid> SeedAsync(ISqlConnectionFactory sqlConnectionFactory)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        // Insert user
        const string insertUserSql = """
            INSERT INTO users (id, first_name, last_name, email, identity_id)
            VALUES (@Id, @FirstName, @LastName, @Email, @IdentityId)
            """;

        await connection.ExecuteAsync(insertUserSql, new
        {
            Id = TestUserId,
            FirstName = TestFirstName,
            LastName = TestLastName,
            Email = TestEmail,
            IdentityId = TestIdentityId
        });

        // Assign "Registered" role (id = 1)
        const string assignRoleSql = """
            INSERT INTO role_user (roles_id, users_id)
            VALUES (1, @UserId)
            """;

        await connection.ExecuteAsync(assignRoleSql, new
        {
            UserId = TestUserId
        });

        return TestUserId;
    }
}