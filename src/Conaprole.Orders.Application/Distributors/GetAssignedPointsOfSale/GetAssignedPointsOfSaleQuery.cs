using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;

public sealed record GetAssignedPointsOfSaleQuery(string DistributorPhoneNumber) : IQuery<List<PointOfSaleResponse>>;