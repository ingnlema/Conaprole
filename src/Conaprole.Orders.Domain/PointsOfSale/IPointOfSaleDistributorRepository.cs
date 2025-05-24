using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.PointsOfSale;

public interface IPointOfSaleDistributorRepository
{
    Task<bool> ExistsAsync(Guid pointOfSaleId, Guid distributorId, Category category, CancellationToken ct = default);
    Task AssignAsync(PointOfSaleDistributor assignment, CancellationToken ct = default);
    Task RemoveAsync(Guid pointOfSaleId, Guid distributorId, Category category, CancellationToken ct = default);
    Task<IReadOnlyCollection<PointOfSaleDistributor>> GetByPointOfSaleIdAsync(Guid pointOfSaleId, CancellationToken ct = default);
    Task<IReadOnlyCollection<PointOfSaleDistributor>> GetByDistributorIdAsync(Guid distributorId, CancellationToken ct = default);
}

