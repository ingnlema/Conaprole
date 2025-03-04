using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Infrastructure.Repositories;

internal sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}