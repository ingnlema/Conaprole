using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.AssignDistributor;

/// <summary>2
/// Comando para asignar un distribuidor a un punto de venta, filtrado por categoría.
/// </summary>
public sealed record AssignDistributorToPointOfSaleCommand(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    Category Category
) : ICommand<bool>;