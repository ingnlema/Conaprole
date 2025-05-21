
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.Orders;

public class Distributor : Entity, IAggregateRoot
{
    public string PhoneNumber { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }

    public ICollection<Category> SupportedCategories { get; private set; } = new List<Category>();
    public ICollection<PointOfSaleDistributor> PointSales { get; private set; } = new List<PointOfSaleDistributor>();

    private Distributor() {}

    public Distributor(Guid id, string phoneNumber, string name, string address)
        : base(id)
    {
        PhoneNumber = phoneNumber;
        Name = name;
        Address = address;
    }

    public void AddCategory(Category category)
    {
        if (!SupportedCategories.Contains(category))
        {
            SupportedCategories.Add(category);
        }
    }

    public void RemoveCategory(Category category)
    {
        SupportedCategories.Remove(category);
    }
}
