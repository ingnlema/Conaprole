using Conaprole.Orders.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Repositories;

internal sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
    
    public async Task<Product?> GetByExternalIdAsync(ExternalProductId externalId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Products
            .FirstOrDefaultAsync(p => p.ExternalProductId == externalId, cancellationToken);

    }
}