using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.Controllers.Products;

public sealed record CreateProductRequest(
    string ExternalProductId,
    string Name,
    decimal UnitPrice,
    string CurrencyCode,
    string Description,
    Category Category
);