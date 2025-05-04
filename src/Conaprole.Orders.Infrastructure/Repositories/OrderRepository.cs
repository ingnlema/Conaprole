
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Infrastructure.Repositories;

        internal sealed class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<Order>()
                .Include(o => o.OrderLines)
                    .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task<Guid?> AddOrderLineAsync(
            Guid orderId,
            ExternalProductId externalProductId,
            int quantity,
            DateTime createdOn,
            CancellationToken ct = default)
        {
            var order = await GetByIdAsync(orderId, ct);
            if (order is null)
                return null;

            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.ExternalProductId == externalProductId, ct);
            if (product is null)
                return null;

            var qty = new Quantity(quantity);
            var line = new OrderLine(
                Guid.NewGuid(),
                qty,
                product.UnitPrice * qty,
                product,
                new OrderId(order.Id),
                createdOn);

            order.AddOrderLine(line);
            _dbContext.Set<OrderLine>().Add(line);
            return line.Id;
        }

        public async Task<bool> RemoveOrderLineAsync(
            Guid orderId,
            Guid orderLineId,
            CancellationToken ct = default)
        {
            var order = await GetByIdAsync(orderId, ct);
            var line = order?.OrderLines.SingleOrDefault(l => l.Id == orderLineId);
            if (order is null || line is null)
                return false;

            order.RemoveOrderLine(orderLineId);
            _dbContext.Set<OrderLine>().Remove(line);
            return true;
        }

        public async Task<bool> UpdateOrderLineQuantityAsync(
            Guid orderId,
            Guid orderLineId,
            int newQuantity,
            CancellationToken ct = default)
        {
            var order = await GetByIdAsync(orderId, ct);
            var line = order?.OrderLines.SingleOrDefault(l => l.Id == orderLineId);
            if (order is null || line is null)
                return false;

            order.UpdateOrderLineQuantity(orderLineId, new Quantity(newQuantity));
            return true;
        }

        public void Add(Order order)
        {
            _dbContext.Set<Order>().Add(order);
        }
    }

