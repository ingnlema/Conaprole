using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.PointsOfSale.EnablePointOfSale;

public sealed record EnablePointOfSaleCommand(string PointOfSalePhoneNumber) : ICommand<bool>;