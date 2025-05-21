
using Conaprole.Orders.Domain.Products;

namespace Conaprole.Orders.Domain.Orders
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<Guid?> AddOrderLineAsync(
            Guid orderId,
            ExternalProductId externalProductId,
            int quantity,
            DateTime createdOn,
            CancellationToken cancellationToken = default);
        Task<bool> RemoveOrderLineAsync(
            Guid orderId,
            Guid orderLineId,
            CancellationToken cancellationToken = default);
        Task<bool> UpdateOrderLineQuantityAsync(
            Guid orderId,
            Guid orderLineId,
            int newQuantity,
            CancellationToken cancellationToken = default);
        void Add(Order order);
    }
}