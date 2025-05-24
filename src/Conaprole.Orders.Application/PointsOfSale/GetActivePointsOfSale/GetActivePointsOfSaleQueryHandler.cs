using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

internal sealed class GetActivePointsOfSaleQueryHandler 
    : IQueryHandler<GetActivePointsOfSaleQuery, List<PointOfSaleResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetActivePointsOfSaleQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<PointOfSaleResponse>>> Handle(GetActivePointsOfSaleQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                phone_number AS PhoneNumber,
                address AS Address,
                is_active AS IsActive,
                created_at AS CreatedAt
            FROM point_of_sale
            WHERE is_active = TRUE
            ORDER BY name;";

        var pointsOfSale = await connection.QueryAsync<PointOfSaleResponse>(sql);
        return Result.Success(pointsOfSale.ToList());
    }
}