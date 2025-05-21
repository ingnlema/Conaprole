using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders;

public class PointOfSale : Entity, IAggregateRoot
{
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<PointOfSaleDistributor> Distributors { get; private set; } = new List<PointOfSaleDistributor>();

    private PointOfSale() { }

    public PointOfSale(Guid id, string phoneNumber, string address) : base(id)
    {
        Id = id;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
