using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Domain.Orders;

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
                delivery_address_zipcode AS DeliveryAddressZipCode, 
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
            WHERE id = @OrderId;

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
                c.category AS Category
            FROM order_lines ol
            INNER JOIN products p ON ol.product_id = p.id
            LEFT JOIN product_categories c ON p.id = c.product_id
            WHERE ol.order_id = @OrderId;
        ";

        using var multi = await connection.QueryMultipleAsync(sql, new { OrderId = request.OrderId });

        var order = await multi.ReadSingleOrDefaultAsync<OrderResponse>();

        if (order is null)
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
                        Categories = group
                            .Where(x => x.Category != null)
                            .Select(x => x.Category!)
                            .Distinct()
                            .ToList()
                    }
                };
            })
            .ToList();
        
        var fullOrder = new OrderResponse
        {
            Id = order.Id,
            PointOfSaleId = order.PointOfSaleId,
            Distributor = order.Distributor,
            DeliveryAddressCity = order.DeliveryAddressCity,
            DeliveryAddressStreet = order.DeliveryAddressStreet,
            DeliveryAddressZipCode = order.DeliveryAddressZipCode,
            Status = order.Status,
            CreatedOnUtc = order.CreatedOnUtc,
            ConfirmedOnUtc = order.ConfirmedOnUtc,
            RejectedOnUtc = order.RejectedOnUtc,
            DeliveryOnUtc = order.DeliveryOnUtc,
            CanceledOnUtc = order.CanceledOnUtc,
            DeliveredOnUtc = order.DeliveredOnUtc,
            PriceAmount = order.PriceAmount,
            PriceCurrency = order.PriceCurrency,
            OrderLines = orderLines
        };
        
        return fullOrder;
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
        public string? Category { get; init; }
    }
}

