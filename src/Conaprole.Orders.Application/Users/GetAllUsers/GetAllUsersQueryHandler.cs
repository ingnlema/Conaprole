using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetAllUsers;

internal sealed class GetAllUsersQueryHandler
    : IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserSummaryResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetAllUsersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<UserSummaryResponse>>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        // Base query to get users
        var sql = """
                  SELECT DISTINCT
                      u.id AS Id,
                      u.email AS Email,
                      u.first_name AS FirstName,
                      u.last_name AS LastName
                  FROM users u
                  """;

        // Add role filter if specified
        if (!string.IsNullOrEmpty(request.RoleFilter))
        {
            sql += """
                   
                   INNER JOIN role_user ru ON u.id = ru.users_id
                   INNER JOIN roles r ON ru.roles_id = r.id
                   WHERE r.name = @RoleFilter
                   """;
        }

        sql += " ORDER BY u.id DESC";

        var users = await connection.QueryAsync<UserSummaryResponse>(
            sql,
            new { RoleFilter = request.RoleFilter });

        // Get roles for each user
        var userList = users.ToList();
        if (userList.Count > 0)
        {
            var userIds = userList.Select(u => u.Id).ToArray();
            
            const string rolesSql = """
                                    SELECT 
                                        u.id as UserId,
                                        r.name as RoleName
                                    FROM users u
                                    INNER JOIN role_user ru ON u.id = ru.users_id
                                    INNER JOIN roles r ON ru.roles_id = r.id
                                    WHERE u.id = ANY(@UserIds)
                                    """;

            var userRoles = await connection.QueryAsync<UserRoleResult>(
                rolesSql,
                new { UserIds = userIds });

            var rolesByUser = userRoles.GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.Select(ur => ur.RoleName).ToList());

            // Update users with their roles
            foreach (var user in userList)
            {
                if (rolesByUser.TryGetValue(user.Id, out var roles))
                {
                    user.Roles.AddRange(roles);
                }
            }
        }

        return Result.Success<IReadOnlyList<UserSummaryResponse>>(userList);
    }

    private sealed class UserRoleResult
    {
        public Guid UserId { get; init; }
        public string RoleName { get; init; } = string.Empty;
    }
}