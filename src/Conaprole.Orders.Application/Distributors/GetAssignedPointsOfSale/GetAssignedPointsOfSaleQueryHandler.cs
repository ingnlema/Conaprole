using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;
using Dapper;

namespace Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;

public sealed class GetAssignedPointsOfSaleQueryHandler : IQueryHandler<GetAssignedPointsOfSaleQuery, List<PointOfSaleResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetAssignedPointsOfSaleQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<PointOfSaleResponse>>> Handle(GetAssignedPointsOfSaleQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT DISTINCT
                pos.id AS Id,
                pos.name AS Name,
                pos.phone_number AS PhoneNumber,
                pos.address AS AddressString,
                pos.is_active AS IsActive,
                pos.created_at AS CreatedAt
            FROM point_of_sale pos
            INNER JOIN point_of_sale_distributor psd ON psd.point_of_sale_id = pos.id
            INNER JOIN distributor d ON d.id = psd.distributor_id
            WHERE d.phone_number = @DistributorPhoneNumber;
        ";

        var rawResults = await connection.QueryAsync<PointOfSaleRawData>(sql, new { request.DistributorPhoneNumber });
        
        var pointsOfSale = rawResults.Select(raw => new PointOfSaleResponse
        {
            Id = raw.Id,
            Name = raw.Name,
            PhoneNumber = raw.PhoneNumber,
            Address = Address.FromString(raw.AddressString),
            IsActive = raw.IsActive,
            CreatedAt = raw.CreatedAt
        }).ToList();
        
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