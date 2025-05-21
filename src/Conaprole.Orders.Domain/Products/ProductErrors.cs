using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Products;

public static class ProductErrors
{
    public static readonly Error NotFound = new(
        "Product.NotFound",
        "The product with the specified identifier was not found");
    
    public static readonly Error DuplicatedExternalId = new(
        "Product.DuplicatedExternalId",
        "A product with the same external product ID already exists");
    
    public static readonly Error InvalidCategory = new(
        "Product.InvalidCategory",
        "The provided category is invalid");
}