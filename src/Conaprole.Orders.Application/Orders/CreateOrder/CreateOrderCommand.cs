using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    Guid PointOfSaleId,
    string Distributor,
    string City,
    string Street,
    string ZipCode,
    decimal Price,
    string CurrencyCode) : ICommand<Guid>;