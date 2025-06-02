using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Repositories
{
    internal sealed class PointOfSaleRepository : IPointOfSaleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PointOfSaleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PointOfSale?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _dbContext.Set<PointOfSale>()
                .Include(p => p.Distributors)
                .ThenInclude(psd => psd.Distributor)
                .FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber, ct);
        }

        public async Task<IReadOnlyCollection<Distributor>> GetDistributorsWithCategoriesAsync(string posPhoneNumber, CancellationToken ct = default)
        {
            return await _dbContext.Set<PointOfSaleDistributor>()
                .Include(psd => psd.Distributor)
                .Where(psd => psd.PointOfSale.PhoneNumber == posPhoneNumber)
                .Select(psd => psd.Distributor)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task AddAsync(PointOfSale pos, CancellationToken ct = default)
        {
            await _dbContext.Set<PointOfSale>().AddAsync(pos, ct);
        }

        public async Task UpdateAsync(PointOfSale pos, CancellationToken ct = default)
        {
            // No-op: Entity is already tracked by EF Core ChangeTracker after retrieval from GetByPhoneNumberAsync
            // Domain methods modify the entity and EF automatically detects changes
            // Only SaveChangesAsync() is needed for persistence
            await Task.CompletedTask;
        }
    }
}