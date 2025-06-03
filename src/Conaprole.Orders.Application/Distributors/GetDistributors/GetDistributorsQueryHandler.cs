using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Dapper;

namespace Conaprole.Orders.Application.Distributors.GetDistributors;

internal sealed class GetDistributorsQueryHandler : IQueryHandler<GetDistributorsQuery, List<DistributorSummaryResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetDistributorsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<List<DistributorSummaryResponse>>> Handle(GetDistributorsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                d.id AS Id,
                d.phone_number AS PhoneNumber,
                d.name AS Name,
                d.address AS Address,
                d.created_at AS CreatedAt,
                d.supported_categories AS SupportedCategoriesRaw,
                COALESCE(pos_count.count, 0) AS AssignedPointsOfSaleCount
            FROM distributor d
            LEFT JOIN (
                SELECT 
                    distributor_id, 
                    COUNT(DISTINCT point_of_sale_id) as count
                FROM point_of_sale_distributor 
                GROUP BY distributor_id
            ) pos_count ON d.id = pos_count.distributor_id
            ORDER BY d.created_at DESC
        """;

        var distributors = await connection.QueryAsync<DistributorDto>(sql);
        
        var result = distributors.Select(d => new DistributorSummaryResponse
        {
            Id = d.Id,
            PhoneNumber = d.PhoneNumber,
            Name = d.Name,
            Address = d.Address,
            CreatedAt = d.CreatedAt,
            SupportedCategories = string.IsNullOrEmpty(d.SupportedCategoriesRaw) 
                ? new List<string>() 
                : d.SupportedCategoriesRaw.Split(',').ToList(),
            AssignedPointsOfSaleCount = d.AssignedPointsOfSaleCount
        }).ToList();

        return Result.Success(result);
    }

    private class DistributorDto
    {
        public Guid Id { get; init; }
        public string PhoneNumber { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string SupportedCategoriesRaw { get; init; } = string.Empty;
        public int AssignedPointsOfSaleCount { get; init; }
    }
}