using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;

public sealed record UnassignDistributorFromPointOfSaleCommand(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    Category Category
) : ICommand<bool>;