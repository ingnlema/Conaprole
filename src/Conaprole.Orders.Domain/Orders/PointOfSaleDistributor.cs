
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.Orders;

public class PointOfSaleDistributor : Entity
{
    public Guid PointOfSaleId { get; private set; }
    public PointOfSale PointOfSale { get; private set; }

    public Guid DistributorId { get; private set; }
    public Distributor Distributor { get; private set; }

    public Category Category { get; private set; }

    public DateTime AssignedAt { get; private set; }

    private PointOfSaleDistributor() {}

    public PointOfSaleDistributor(Guid id, Guid posId, Guid distId, Category category)
        : base(id)
    {
        PointOfSaleId = posId;
        DistributorId = distId;
        Category = category;
        AssignedAt = DateTime.UtcNow;
    }
}