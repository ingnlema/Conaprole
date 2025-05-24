
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;


namespace Conaprole.Orders.Infrastructure.Repositories;


internal sealed class PointOfSaleDistributorRepository : IPointOfSaleDistributorRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PointOfSaleDistributorRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(Guid pointOfSaleId, Guid distributorId, Category category, CancellationToken ct = default)
    {
        return await _dbContext.Set<PointOfSaleDistributor>().AnyAsync(
            x => x.PointOfSaleId == pointOfSaleId &&
                 x.DistributorId == distributorId &&
                 x.Category == category,
            ct);
    }

    public async Task AssignAsync(PointOfSaleDistributor assignment, CancellationToken ct = default)
    {
        await _dbContext.Set<PointOfSaleDistributor>().AddAsync(assignment, ct);
    }

    public async Task RemoveAsync(Guid pointOfSaleId, Guid distributorId, Category category, CancellationToken ct = default)
    {
        var assignment = await _dbContext.Set<PointOfSaleDistributor>()
            .FirstOrDefaultAsync(x =>
                x.PointOfSaleId == pointOfSaleId &&
                x.DistributorId == distributorId &&
                x.Category == category, ct);

        if (assignment != null)
        {
            _dbContext.Set<PointOfSaleDistributor>().Remove(assignment);
        }
    }

    public async Task<IReadOnlyCollection<PointOfSaleDistributor>> GetByPointOfSaleIdAsync(Guid pointOfSaleId, CancellationToken ct = default)
    {
        return await _dbContext.Set<PointOfSaleDistributor>()
            .Include(x => x.Distributor)
            .Where(x => x.PointOfSaleId == pointOfSaleId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<PointOfSaleDistributor>> GetByDistributorIdAsync(Guid distributorId, CancellationToken ct = default)
    {
        return await _dbContext.Set<PointOfSaleDistributor>()
            .Include(x => x.PointOfSale)
            .Where(x => x.DistributorId == distributorId)
            .ToListAsync(ct);
    }
}
