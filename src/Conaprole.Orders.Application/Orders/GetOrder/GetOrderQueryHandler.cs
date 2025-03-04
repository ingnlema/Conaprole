namespace Conaprole.Orders.Application.Orders.GetOrder;

using Abstractions.Data;
using Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;


internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetOrderQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                id AS Id,
                point_of_sale_id AS PointOfSaleId,
                distributor AS Distributor,
                delivery_address_city AS DeliveryAddressCity,
                delivery_address_street AS DeliveryAddressStreet,
                delivery_address_zip_code AS DeliveryAddressZipCode,
                status AS Status,
                created_on_utc AS CreatedOnUtc,
                confirmed_on_utc AS ConfirmedOnUtc,
                rejected_on_utc AS RejectedOnUtc,
                delivery_on_utc AS DeliveryOnUtc,
                canceled_on_utc AS CanceledOnUtc,
                delivered_on_utc AS DeliveredOnUtc,
                price_amount AS PriceAmount,
                price_currency AS PriceCurrency
            FROM orders
            WHERE id = @OrderId";

        var order = await connection.QueryFirstOrDefaultAsync<OrderResponse>(
            sql,
            new { OrderId = request.OrderId }
        );

        if (order is null)
        {
            return Result.Failure<OrderResponse>(
                new Error("Order.NotFound", "El pedido no fue encontrado.")
            );
        }
        return order;
    }
}
