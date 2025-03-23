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
                               id AS Id,
                               first_name AS FirstName,
                               last_name AS LastName,
                               email AS Email
                           FROM users
                           WHERE identity_id = @IdentityId
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

        return user;
    }
}