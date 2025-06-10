using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Distributors.GetOrdersForDistributor;

internal sealed class GetOrdersForDistributorQueryHandler : IQueryHandler<GetOrdersForDistributorQuery, List<OrderSummaryResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetOrdersForDistributorQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<OrderSummaryResponse>>> Handle(GetOrdersForDistributorQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = """
            SELECT
            o.id AS Id,
            o.status AS Status,
            CASE o.status
                  WHEN 0 THEN 'Created'
                  WHEN 1 THEN 'Confirmed'
                  WHEN 2 THEN 'Delivered'
                  WHEN -1 THEN 'Rejected'
                  WHEN -2 THEN 'Canceled'
            END AS StatusName,             
            o.created_on_utc AS CreatedOnUtc,
            d.phone_number AS DistributorPhoneNumber,
            pos.phone_number AS PointOfSalePhoneNumber,
            o.delivery_address_city AS City,
            o.delivery_address_street AS Street,
            o.delivery_address_zipcode AS ZipCode,
            o.price_amount AS PriceAmount,
            o.price_currency AS PriceCurrency
        FROM orders o
        JOIN distributor d ON d.id = o.distributor_id
        JOIN point_of_sale pos ON pos.id = o.point_of_sale_id
        WHERE d.phone_number ILIKE @DistributorPhoneNumber
        """;

        var parameters = new DynamicParameters();
        parameters.Add("DistributorPhoneNumber", $"%{request.DistributorPhoneNumber}%");

        if (!string.IsNullOrWhiteSpace(request.PointOfSalePhoneNumber))
        {
            sql += " AND pos.phone_number = @PointOfSalePhoneNumber";
            parameters.Add("PointOfSalePhoneNumber", request.PointOfSalePhoneNumber);
        }

        sql += " ORDER BY created_on_utc DESC";

        var orders = await connection.QueryAsync<OrderSummaryResponse>(sql, parameters);
        return Result.Success(orders?.ToList() ?? new List<OrderSummaryResponse>());
    }
}