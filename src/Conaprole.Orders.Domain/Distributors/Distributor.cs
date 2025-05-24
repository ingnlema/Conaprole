using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.Distributors;

public class Distributor : Entity, IAggregateRoot
{
    public string PhoneNumber { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Category> SupportedCategories { get; private set; } = new List<Category>();
    public ICollection<PointOfSaleDistributor> PointSales { get; private set; } = new List<PointOfSaleDistributor>();

    private Distributor() {}

    public Distributor(Guid id, string name, string phoneNumber, string address, DateTime createdAt, IEnumerable<Category> categories)
        : base(id)
    {
        Name = name;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = createdAt;
        SupportedCategories = new List<Category>(categories);
    }

    public bool AddCategory(Category category)
    {
        if (SupportedCategories.Contains(category))
            return false;

        SupportedCategories.Add(category);
        return true;
    }

    public bool RemoveCategory(Category category)
    {
        if (!SupportedCategories.Contains(category))
            return false;

        SupportedCategories.Remove(category);
        return true;
    }
}
