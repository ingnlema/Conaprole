using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;

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
        o.id AS Id,
        pos.phone_number AS PointOfSalePhoneNumber,
        d.phone_number AS DistributorPhoneNumber,
        o.delivery_address_city AS DeliveryAddressCity,
        o.delivery_address_street AS DeliveryAddressStreet,
        o.delivery_address_zipcode AS DeliveryAddressZipCode, 
        o.status AS Status,
        o.created_on_utc AS CreatedOnUtc,
        o.confirmed_on_utc AS ConfirmedOnUtc,
        o.rejected_on_utc AS RejectedOnUtc,
        o.delivery_on_utc AS DeliveryOnUtc,
        o.canceled_on_utc AS CanceledOnUtc,
        o.delivered_on_utc AS DeliveredOnUtc,
        o.price_amount AS PriceAmount,
        o.price_currency AS PriceCurrency
        FROM orders o
        JOIN point_of_sale pos ON pos.id = o.point_of_sale_id
        JOIN distributor d ON d.id = o.distributor_id
        WHERE o.id = @OrderId;

        SELECT 
        ol.id AS Id,
        ol.quantity AS Quantity,
        ol.sub_total_amount AS SubTotal,
        ol.order_id AS OrderId,
        ol.created_on_utc AS CreatedOnUtc,
        p.id AS ProductId,
        p.external_product_id AS ExternalProductId,
        p.name AS Name,
        p.description AS Description,
        p.unit_price_amount AS UnitPrice,
        p.last_updated AS LastUpdated,
        p.category AS Category
        FROM order_lines ol
        INNER JOIN products p ON ol.product_id = p.id
        WHERE ol.order_id = @OrderId;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { OrderId = request.OrderId });

        var dbOrder = await multi.ReadSingleOrDefaultAsync<OrderResponse>();

        if (dbOrder is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        }

        var lines = await multi.ReadAsync<OrderLineFlat>();
        
        var orderLines = lines
            .GroupBy(x => x.Id)
            .Select(group =>
            {
                var first = group.First();
                return new OrderLineResponse
                {
                    Id = first.Id,
                    Quantity = first.Quantity,
                    SubTotal = first.SubTotal,
                    OrderId = first.OrderId,
                    CreatedOnUtc = first.CreatedOnUtc,
                    ProductId = first.ProductId,
                    Product = new ProductResponse
                    {
                        Id = first.ProductId,
                        ExternalProductId = first.ExternalProductId,
                        Name = first.Name,
                        Description = first.Description,
                        UnitPrice = first.UnitPrice,
                        LastUpdated = first.LastUpdated,
                        Category = first.Category.ToString()
                    }
                };
            })
            .ToList();
        
        var fullOrder = new OrderResponse
        {
            Id = dbOrder.Id,
            PointOfSalePhoneNumber = dbOrder.PointOfSalePhoneNumber,
            DistributorPhoneNumber = dbOrder.DistributorPhoneNumber,
            DeliveryAddressCity = dbOrder.DeliveryAddressCity,
            DeliveryAddressStreet = dbOrder.DeliveryAddressStreet,
            DeliveryAddressZipCode = dbOrder.DeliveryAddressZipCode,
            Status = dbOrder.Status,
            CreatedOnUtc = dbOrder.CreatedOnUtc,
            ConfirmedOnUtc = dbOrder.ConfirmedOnUtc,
            RejectedOnUtc = dbOrder.RejectedOnUtc,
            DeliveryOnUtc = dbOrder.DeliveryOnUtc,
            CanceledOnUtc = dbOrder.CanceledOnUtc,
            DeliveredOnUtc = dbOrder.DeliveredOnUtc,
            PriceAmount = dbOrder.PriceAmount,
            PriceCurrency = dbOrder.PriceCurrency,
            OrderLines = orderLines
        };
        
        return Result.Success( fullOrder);
    }


    private class OrderLineFlat
    {
        public Guid Id { get; init; }
        public int Quantity { get; init; }
        public decimal SubTotal { get; init; }
        public Guid OrderId { get; init; }
        public DateTime CreatedOnUtc { get; init; }
        public Guid ProductId { get; init; }
        public string ExternalProductId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public DateTime LastUpdated { get; init; }
        public Category Category { get; init; }
    }
}

