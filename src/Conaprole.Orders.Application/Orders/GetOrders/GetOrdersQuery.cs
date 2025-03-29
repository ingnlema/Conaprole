using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.GetOrders;

public sealed record GetOrdersQuery(
    DateTime? From,
    DateTime? To,
    int? Status,
    string? Distributor,
    Guid? PointOfSaleId
) : IQuery<List<OrderSummaryResponse>>;
