namespace Conaprole.Orders.Api.Controllers.Products;

public sealed record CreateProductRequest(
    string ExternalProductId,
    string Name,
    decimal UnitPrice,
    string CurrencyCode,
    string Description,
    List<string>? Categories
);