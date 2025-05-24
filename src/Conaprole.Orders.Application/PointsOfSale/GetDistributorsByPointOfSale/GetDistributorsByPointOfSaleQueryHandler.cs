using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;

public sealed class GetDistributorsByPointOfSaleQueryHandler : IQueryHandler<GetDistributorsByPointOfSaleQuery, List<DistributorWithCategoriesResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDistributorsByPointOfSaleQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<DistributorWithCategoriesResponse>>> Handle(GetDistributorsByPointOfSaleQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                d.phone_number AS PhoneNumber,
                d.name AS Name,
                pd.category AS Category
            FROM point_of_sale_distributor pd
            JOIN distributor d ON d.id = pd.distributor_id
            JOIN point_of_sale pos ON pos.id = pd.point_of_sale_id
            WHERE pos.phone_number = @PhoneNumber";

        var data = await connection.QueryAsync<DistributorCategoryDto>(sql, new { PhoneNumber = request.PointOfSalePhoneNumber });

        var grouped = data
            .GroupBy(x => new { x.PhoneNumber, x.Name })
            .Select(g => new DistributorWithCategoriesResponse
            {
                PhoneNumber = g.Key.PhoneNumber,
                Name = g.Key.Name,
                Categories = g.Select(x => x.Category).ToList()
            })
            .ToList();

        return Result.Success(grouped);
    }

    private class DistributorCategoryDto
    {
        public string PhoneNumber { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }
}