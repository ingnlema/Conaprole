using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.PointsOfSale.GetPointsOfSale;

internal sealed class GetPointsOfSaleQueryHandler 
    : IQueryHandler<GetPointsOfSaleQuery, List<PointOfSaleResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetPointsOfSaleQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<PointOfSaleResponse>>> Handle(GetPointsOfSaleQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                phone_number AS PhoneNumber,
                address AS Address,
                is_active AS IsActive,
                created_at AS CreatedAt
            FROM point_of_sale";

        var parameters = new DynamicParameters();

        // Add filtering based on status parameter
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            switch (request.Status.ToLowerInvariant())
            {
                case "active":
                    sql += " WHERE is_active = TRUE";
                    break;
                case "inactive":
                    sql += " WHERE is_active = FALSE";
                    break;
                case "all":
                    // No filter - return all POS
                    break;
                default:
                    // Default to active for invalid values to maintain backward compatibility
                    sql += " WHERE is_active = TRUE";
                    break;
            }
        }
        else
        {
            // Default to active when no status is provided
            sql += " WHERE is_active = TRUE";
        }

        sql += " ORDER BY name";

        var pointsOfSale = await connection.QueryAsync<PointOfSaleResponse>(sql, parameters);
        return Result.Success(pointsOfSale.ToList());
    }
}