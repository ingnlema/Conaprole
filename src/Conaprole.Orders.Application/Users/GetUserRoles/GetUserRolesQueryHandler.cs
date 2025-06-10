using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetUserRoles;

internal sealed class GetUserRolesQueryHandler
    : IQueryHandler<GetUserRolesQuery, IReadOnlyList<RoleResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetUserRolesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<RoleResponse>>> Handle(
        GetUserRolesQuery request,
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
            return Result.Failure<IReadOnlyList<RoleResponse>>(UserErrors.NotFound);
        }

        // Get user roles
        const string sql = """
                           SELECT
                               r.id AS Id,
                               r.name AS Name
                           FROM roles r
                           INNER JOIN role_user ru ON r.id = ru.roles_id
                           INNER JOIN users u ON ru.users_id = u.id
                           WHERE u.id = @UserId
                           ORDER BY r.id
                           """;

        var roles = await connection.QueryAsync<RoleResponse>(
            sql,
            new { UserId = request.UserId });

        return Result.Success<IReadOnlyList<RoleResponse>>(roles.ToList());
    }
}