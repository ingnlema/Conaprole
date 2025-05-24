using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Repositories
{
    internal sealed class DistributorRepository : IDistributorRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DistributorRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Distributor?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _dbContext.Set<Distributor>()
                .Include(d => d.PointSales)
                .ThenInclude(psd => psd.PointOfSale)
                .FirstOrDefaultAsync(d => d.PhoneNumber == phoneNumber, ct);
        }

        public async Task<IReadOnlyCollection<PointOfSale>> GetAssignedPointsOfSaleAsync(string distributorPhone, CancellationToken ct = default)
        {
            return await _dbContext.Set<PointOfSaleDistributor>()
                .Include(psd => psd.PointOfSale)
                .Where(psd => psd.Distributor.PhoneNumber == distributorPhone)
                .Select(psd => psd.PointOfSale)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task AddAsync(Distributor distributor, CancellationToken ct = default)
        {
            await _dbContext.Set<Distributor>().AddAsync(distributor, ct);
        }
    }
}