using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

public record CreateOrderCommand(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<CreateOrderLineCommand> OrderLines) : ICommand<Guid>;