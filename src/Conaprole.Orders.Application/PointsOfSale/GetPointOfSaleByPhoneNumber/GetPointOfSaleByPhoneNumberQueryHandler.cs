using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Dapper;

namespace Conaprole.Orders.Application.PointsOfSale.GetPointOfSaleByPhoneNumber;

internal sealed class GetPointOfSaleByPhoneNumberQueryHandler 
    : IQueryHandler<GetPointOfSaleByPhoneNumberQuery, PointOfSaleResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetPointOfSaleByPhoneNumberQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<PointOfSaleResponse>> Handle(GetPointOfSaleByPhoneNumberQuery request, CancellationToken cancellationToken)
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
            WHERE phone_number = @PhoneNumber";

        var pointOfSale = await connection.QuerySingleOrDefaultAsync<PointOfSaleResponse>(
            sql, 
            new { PhoneNumber = request.PhoneNumber });

        if (pointOfSale is null)
        {
            return Result.Failure<PointOfSaleResponse>(PointOfSaleErrors.NotFound);
        }

        return Result.Success(pointOfSale);
    }
}