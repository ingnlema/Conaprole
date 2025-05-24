using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Dapper;

namespace Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;

internal sealed class GetPointOfSaleDetailsQueryHandler : IQueryHandler<GetPointOfSaleDetailsQuery, PointOfSaleDetailsResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetPointOfSaleDetailsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<PointOfSaleDetailsResponse>> Handle(GetPointOfSaleDetailsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                pos.name AS Name,
                pos.phone_number AS PhoneNumber,
                pos.address AS Address,
                pos.is_active AS IsActive,
                pos.created_at AS CreatedAt
            FROM point_of_sale pos
            JOIN point_of_sale_distributor posd ON pos.id = posd.point_of_sale_id
            JOIN distributor d ON d.id = posd.distributor_id
            WHERE pos.phone_number = @PointOfSalePhoneNumber AND d.phone_number = @DistributorPhoneNumber;
            """;

        var pointOfSale = await connection.QuerySingleOrDefaultAsync<PointOfSaleDetailsResponse>(
            sql, new
            {
                PointOfSalePhoneNumber = request.PointOfSalePhoneNumber,
                DistributorPhoneNumber = request.DistributorPhoneNumber
            });

        if (pointOfSale is null)
        {
            return Result.Failure<PointOfSaleDetailsResponse>(PointOfSaleErrors.NotFound);
        }

        return Result.Success(pointOfSale);
    }
}