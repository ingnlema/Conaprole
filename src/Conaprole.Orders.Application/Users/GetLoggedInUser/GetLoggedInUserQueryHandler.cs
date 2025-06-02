using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using Dapper;

namespace Conaprole.Orders.Application.Users.GetLoggedInUser;

internal sealed class GetLoggedInUserQueryHandler
    : IQueryHandler<GetLoggedInUserQuery, UserResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetLoggedInUserQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<UserResponse>> Handle(
        GetLoggedInUserQuery request,
        CancellationToken cancellationToken)
    {
        
        if (string.IsNullOrWhiteSpace(_userContext.IdentityId))
        {
            return Result.Failure<UserResponse>(UserErrors.IdentityMissing);
        }
        
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
                           SELECT
                               u.id AS Id,
                               u.first_name AS FirstName,
                               u.last_name AS LastName,
                               u.email AS Email,
                               u.distributor_id AS DistributorId,
                               d.phone_number AS DistributorPhoneNumber
                           FROM users u
                           LEFT JOIN distributor d ON u.distributor_id = d.id
                           WHERE u.identity_id = @IdentityId
                           """;

        var user = await connection.QuerySingleOrDefaultAsync<UserResponse>(
            sql,
            new
            {
                IdentityId = _userContext.IdentityId
            });

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        // Get user roles
        const string rolesSql = """
                                SELECT r.name
                                FROM roles r
                                INNER JOIN role_user ru ON r.id = ru.roles_id
                                INNER JOIN users u ON ru.users_id = u.id
                                WHERE u.identity_id = @IdentityId
                                """;

        var roles = await connection.QueryAsync<string>(
            rolesSql,
            new
            {
                IdentityId = _userContext.IdentityId
            });

        // Update the user response with roles
        var userWithRoles = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DistributorId = user.DistributorId,
            DistributorPhoneNumber = user.DistributorPhoneNumber,
            Roles = roles.ToList()
        };

        return Result.Success(userWithRoles);
    }
}