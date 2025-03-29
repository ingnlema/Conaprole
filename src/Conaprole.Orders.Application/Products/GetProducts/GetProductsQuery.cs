using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Products.GetProducts;

public sealed record GetProductsQuery : IQuery<List<ProductsResponse>>;