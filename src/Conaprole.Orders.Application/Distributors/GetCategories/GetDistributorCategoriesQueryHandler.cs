using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Distributors.GetCategories;

public sealed class GetDistributorCategoriesQueryHandler : IQueryHandler<GetDistributorCategoriesQuery, List<string>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDistributorCategoriesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<string>>> Handle(GetDistributorCategoriesQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT unnest(string_to_array(supported_categories, ',')) as category
            FROM distributor 
            WHERE phone_number = @PhoneNumber
        """;

        var categories = await connection.QueryAsync<string>(sql, new { PhoneNumber = request.DistributorPhoneNumber });

        return Result.Success(categories.ToList());
    }
}