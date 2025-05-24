using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;

public sealed record CreatePointOfSaleCommand(
    string Name,
    string PhoneNumber,
    string City,
    string Street,
    string ZipCode
) : ICommand<Guid>;