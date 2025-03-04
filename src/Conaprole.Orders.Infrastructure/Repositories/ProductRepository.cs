using Conaprole.Orders.Domain.Products;

namespace Conaprole.Orders.Infrastructure.Repositories;

internal sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}