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
                id AS Id,
                external_product_id AS ExternalProductId,
                name AS Name,
                description AS Description,
                unit_price_amount AS UnitPrice,
                last_updated AS LastUpdated,
                category AS Category
            FROM products";

        var result = await connection.QueryAsync<ProductsResponse>(sql);
        return Result.Success(result.ToList());
    }
}
