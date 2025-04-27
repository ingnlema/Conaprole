using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Orders.GetOrders;

internal sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, List<OrderSummaryResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetOrdersQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

     public async Task<Result<List<OrderSummaryResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = """
            SELECT
                id                        AS Id,
                status                    AS Status,
                status::text              AS StatusName,             
                created_on_utc            AS CreatedOnUtc,
                distributor               AS Distributor,
                point_of_sale_id AS PointOfSalePhoneNumber, 
                delivery_address_city                      AS City,                   
                delivery_address_street                    AS Street,               
                delivery_address_zipcode                  AS ZipCode,                
                price_amount              AS PriceAmount,
                price_currency            AS PriceCurrency
            FROM orders
            WHERE 1=1
        """;

        var parameters = new DynamicParameters();

        if (request.From.HasValue)
        {
            sql += " AND created_on_utc >= @From";
            parameters.Add("From", request.From.Value);
        }
        if (request.To.HasValue)
        {
            sql += " AND created_on_utc <= @To";
            parameters.Add("To", request.To.Value);
        }
        if (request.Status.HasValue)
        {
            sql += " AND status = @Status";
            parameters.Add("Status", request.Status.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.Distributor))
        {
            sql += " AND distributor ILIKE @Distributor";
            parameters.Add("Distributor", $"%{request.Distributor}%");
        }
        if (!string.IsNullOrWhiteSpace(request.PointOfSalePhoneNumber))
        {
            sql += " AND point_of_sale_id = @PointOfSalePhoneNumber";  
            parameters.Add("PointOfSalePhoneNumber", request.PointOfSalePhoneNumber);
        }

        sql += " ORDER BY created_on_utc DESC";

        var orders = await connection.QueryAsync<OrderSummaryResponse>(sql, parameters);
        return Result.Success(orders.ToList());
    }
}
