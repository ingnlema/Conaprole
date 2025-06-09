using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Application.PointsOfSale.GetPointsOfSale;

public sealed record GetPointsOfSaleQuery(
    string? Status = "active"
) : IQuery<List<PointOfSaleResponse>>;