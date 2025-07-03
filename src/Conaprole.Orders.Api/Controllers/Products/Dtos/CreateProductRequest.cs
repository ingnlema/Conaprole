using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.Controllers.Products;

/// <summary>
/// Request model for creating a new product
/// </summary>
/// <param name="ExternalProductId">External identifier for the product (SKU)</param>
/// <param name="Name">Product name</param>
/// <param name="UnitPrice">Unit price of the product</param>
/// <param name="CurrencyCode">Currency code for the price (e.g., UYU, USD)</param>
/// <param name="Description">Product description</param>
/// <param name="Category">Product category</param>
public sealed record CreateProductRequest(
    string ExternalProductId,
    string Name,
    decimal UnitPrice,
    string CurrencyCode,
    string Description,
    Category Category
);