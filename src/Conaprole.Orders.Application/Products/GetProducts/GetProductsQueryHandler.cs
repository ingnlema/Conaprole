using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;
using Conaprole.Orders.Application.Abstractions.Data;

namespace Conaprole.Orders.Application.Products.GetProducts;

internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, List<ProductsResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetProductsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<ProductsResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                p.id AS Id,
                p.external_product_id AS ExternalProductId,
                p.name AS Name,
                p.description AS Description,
                p.unit_price_amount AS UnitPrice,
                p.last_updated AS LastUpdated,
                c.category AS Category
            FROM products p
            LEFT JOIN product_categories c ON p.id = c.product_id";

        var productMap = new Dictionary<Guid, ProductsResponse>();

        var result = await connection.QueryAsync<ProductFlat, string?, ProductsResponse>(
            sql,
            (flat, category) =>
            {
                if (!productMap.TryGetValue(flat.Id, out var product))
                {
                    product = new ProductsResponse
                    {
                        Id = flat.Id,
                        ExternalProductId = flat.ExternalProductId,
                        Name = flat.Name,
                        Description = flat.Description,
                        UnitPrice = flat.UnitPrice,
                        LastUpdated = flat.LastUpdated,
                        Categories = new List<string>()
                    };
                    productMap.Add(flat.Id, product);
                }

                if (!string.IsNullOrWhiteSpace(category) && !product.Categories.Contains(category))
                {
                    product.Categories.Add(category);
                }

                return product;
            },
            splitOn: "Category" 
        );

        return productMap.Values.ToList();
    }

    private sealed class ProductFlat
    {
        public Guid Id { get; init; }
        public string ExternalProductId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public DateTime LastUpdated { get; init; }
    }
}
