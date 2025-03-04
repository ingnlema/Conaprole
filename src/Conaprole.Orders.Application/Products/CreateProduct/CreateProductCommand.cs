using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Products.CreateProduct;

public record CreateProductCommand(
    string ExternalProductId,
    string Name,
    decimal UnitPrice,
    string CurrencyCode,
    string Description,
    List<string>? Categories
) : ICommand<Guid>;