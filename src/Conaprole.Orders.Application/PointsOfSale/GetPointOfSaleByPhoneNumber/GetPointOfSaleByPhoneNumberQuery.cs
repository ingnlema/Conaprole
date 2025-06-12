using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Application.PointsOfSale.GetPointOfSaleByPhoneNumber;

public sealed record GetPointOfSaleByPhoneNumberQuery(string PhoneNumber) : IQuery<PointOfSaleResponse>;