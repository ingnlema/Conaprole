using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Products;
using Dapper;

namespace Conaprole.Orders.Application.Products.GetProduct;

internal sealed class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetProductQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<ProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                id AS Id,
                external_product_id AS ExternalProductId,
                name AS Name,
                unit_price_amount AS UnitPriceAmount,
                unit_price_currency AS UnitPriceCurrency,
                description AS Description,
                last_updated AS LastUpdated,
                category AS Category
            FROM products
            WHERE id = @ProductId;";

        var product = await connection.QueryFirstOrDefaultAsync<ProductResponse>(
            sql,
            new { ProductId = request.ProductId }
        );

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);
        }
        

        return Result.Success(product);
    }
}