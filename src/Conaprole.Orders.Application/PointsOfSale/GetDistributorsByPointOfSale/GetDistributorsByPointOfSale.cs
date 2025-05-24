using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;

public sealed record GetDistributorsByPointOfSaleQuery(string PointOfSalePhoneNumber) : IQuery<List<DistributorWithCategoriesResponse>>;