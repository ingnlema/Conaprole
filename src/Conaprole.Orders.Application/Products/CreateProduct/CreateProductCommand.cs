using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Products.CreateProduct;

public record CreateProductCommand(
    string ExternalProductId,
    string Name,
    decimal UnitPrice,
    string CurrencyCode,
    string Description,
    Category Category
) : ICommand<Guid>;