using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetAllPermissions;

internal sealed class GetAllPermissionsQueryHandler
    : IQueryHandler<GetAllPermissionsQuery, IReadOnlyList<PermissionResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetAllPermissionsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<IReadOnlyList<PermissionResponse>>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                               p.id AS Id,
                               p.name AS Name
                           FROM permissions p
                           ORDER BY p.id
                           """;

        var permissions = await connection.QueryAsync<PermissionResponse>(sql);

        return Result.Success<IReadOnlyList<PermissionResponse>>(permissions.ToList());
    }
}