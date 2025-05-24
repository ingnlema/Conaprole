using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.PointsOfSale;

public class PointOfSaleDistributor : Entity
{
    public Guid PointOfSaleId { get; private set; }
    public PointOfSale PointOfSale { get; private set; } = null!;

    public Guid DistributorId { get; private set; }
    public Distributor Distributor { get; private set; } = null!;

    public Category Category { get; private set; }

    public DateTime AssignedAt { get; private set; }

    private PointOfSaleDistributor() {}

    public PointOfSaleDistributor(Guid id, Guid pointOfSaleId, Guid distributorId, Category category)
        : base(id)
    {
        PointOfSaleId = pointOfSaleId;
        DistributorId = distributorId;
        Category = category;
        AssignedAt = DateTime.UtcNow;
    }

    public static PointOfSaleDistributor Create(Guid pointOfSaleId, Guid distributorId, Category category)
    {
        return new PointOfSaleDistributor(
            Guid.NewGuid(),
            pointOfSaleId,
            distributorId,
            category
        );
    }
}