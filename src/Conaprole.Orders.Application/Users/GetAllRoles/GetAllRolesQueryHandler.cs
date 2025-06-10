using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetAllRoles;

internal sealed class GetAllRolesQueryHandler
    : IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetAllRolesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<RoleResponse>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                               r.id AS Id,
                               r.name AS Name
                           FROM roles r
                           ORDER BY r.id
                           """;

        var roles = await connection.QueryAsync<RoleResponse>(sql);

        return Result.Success<IReadOnlyList<RoleResponse>>(roles.ToList());
    }
}