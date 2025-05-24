using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

public sealed record GetActivePointsOfSaleQuery : IQuery<List<PointOfSaleResponse>>;