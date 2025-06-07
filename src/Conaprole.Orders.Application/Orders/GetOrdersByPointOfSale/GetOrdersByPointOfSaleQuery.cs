using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Orders.GetOrders;

namespace Conaprole.Orders.Application.Orders.GetOrdersByPointOfSale;

public sealed record GetOrdersByPointOfSaleQuery(
    string PointOfSalePhoneNumber
) : IQuery<List<OrderSummaryResponse>>;