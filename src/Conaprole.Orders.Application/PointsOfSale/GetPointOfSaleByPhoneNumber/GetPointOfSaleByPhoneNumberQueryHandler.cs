using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
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
                address AS AddressString,
                is_active AS IsActive,
                created_at AS CreatedAt
            FROM point_of_sale
            WHERE phone_number = @PhoneNumber";

        var rawResult = await connection.QuerySingleOrDefaultAsync<PointOfSaleRawData>(
            sql, 
            new { PhoneNumber = request.PhoneNumber });

        if (rawResult is null)
        {
            return Result.Failure<PointOfSaleResponse>(PointOfSaleErrors.NotFound);
        }

        var pointOfSale = new PointOfSaleResponse(
            rawResult.Id,
            rawResult.Name,
            rawResult.PhoneNumber,
            Address.FromString(rawResult.AddressString),
            rawResult.IsActive,
            rawResult.CreatedAt
        );

        return Result.Success(pointOfSale);
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