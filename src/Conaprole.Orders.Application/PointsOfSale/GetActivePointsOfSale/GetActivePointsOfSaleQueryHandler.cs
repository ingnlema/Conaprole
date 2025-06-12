using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;
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
                address AS AddressString,
                is_active AS IsActive,
                created_at AS CreatedAt
            FROM point_of_sale
            WHERE is_active = TRUE
            ORDER BY name;";

        var rawResults = await connection.QueryAsync<PointOfSaleRawData>(sql);
        
        var pointsOfSale = rawResults.Select(raw => new PointOfSaleResponse(
            raw.Id,
            raw.Name,
            raw.PhoneNumber,
            Address.FromString(raw.AddressString),
            raw.IsActive,
            raw.CreatedAt
        )).ToList();
        
        return Result.Success(pointsOfSale);
    }

    private class PointOfSaleRawData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressString { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}