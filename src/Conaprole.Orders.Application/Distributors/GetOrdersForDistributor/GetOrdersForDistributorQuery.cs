using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Orders.GetOrders;

namespace Conaprole.Orders.Application.Distributors.GetOrdersForDistributor;

public sealed record GetOrdersForDistributorQuery(
    string DistributorPhoneNumber,
    string? PointOfSalePhoneNumber
) : IQuery<List<OrderSummaryResponse>>;