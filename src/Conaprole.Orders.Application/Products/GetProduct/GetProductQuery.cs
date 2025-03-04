using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Products.GetProduct;

public sealed record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;