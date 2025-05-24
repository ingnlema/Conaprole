using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;

public sealed record DisablePointOfSaleCommand(string PointOfSalePhoneNumber) : ICommand<bool>;