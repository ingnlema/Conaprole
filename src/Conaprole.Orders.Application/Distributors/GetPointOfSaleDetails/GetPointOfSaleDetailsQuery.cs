using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;

public sealed record GetPointOfSaleDetailsQuery(string DistributorPhoneNumber, string PointOfSalePhoneNumber) : IQuery<PointOfSaleDetailsResponse>;