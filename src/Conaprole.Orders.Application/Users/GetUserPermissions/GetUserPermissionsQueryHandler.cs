using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetUserPermissions;

internal sealed class GetUserPermissionsQueryHandler
    : IQueryHandler<GetUserPermissionsQuery, IReadOnlyList<PermissionResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUserPermissionsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<PermissionResponse>>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        // Check if user exists first
        const string userExistsSql = """
                                     SELECT COUNT(1) FROM users WHERE id = @UserId
                                     """;

        var userExists = await connection.QuerySingleAsync<int>(
            userExistsSql,
            new { UserId = request.UserId });

        if (userExists == 0)
        {
            return Result.Failure<IReadOnlyList<PermissionResponse>>(UserErrors.NotFound);
        }

        // Get user permissions through roles
        const string sql = """
                           SELECT DISTINCT
                               p.id AS Id,
                               p.name AS Name
                           FROM permissions p
                           INNER JOIN role_permissions pr ON p.id = pr.permission_id
                           INNER JOIN roles r ON pr.role_id = r.id
                           INNER JOIN role_user ru ON r.id = ru.roles_id
                           INNER JOIN users u ON ru.users_id = u.id
                           WHERE u.id = @UserId
                           ORDER BY p.id
                           """;

        var permissions = await connection.QueryAsync<PermissionResponse>(
            sql,
            new { UserId = request.UserId });

        return Result.Success<IReadOnlyList<PermissionResponse>>(permissions.ToList());
    }
}