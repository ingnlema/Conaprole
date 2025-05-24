using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.PointsOfSale
{

    // Ensure Category enum and PointOfSaleDistributor class are in scope
    using Conaprole.Orders.Domain.Distributors;

    public class PointOfSale : Entity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Address { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public ICollection<PointOfSaleDistributor> Distributors { get; private set; } =
            new List<PointOfSaleDistributor>();

        private PointOfSale()
        {
        }

        public PointOfSale(Guid id, string name, string phoneNumber, Address address, DateTime createdAt) : base(id)
        {
            // Id is already set via base constructor
            Name = name;
            PhoneNumber = phoneNumber;
            Address = address.ToString();
            CreatedAt = createdAt;
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

        public bool AssignDistributor(Guid distributorId, Category category)
        {
            if (Distributors.Any(d => d.DistributorId == distributorId && d.Category == category))
                return false;

            var distributor = PointOfSaleDistributor.Create(Id, distributorId, category);
            Distributors.Add(distributor);
            return true;
        }

        public bool UnassignDistributor(Guid distributorId, Category category)
        {
            var distributor = Distributors.FirstOrDefault(d => d.DistributorId == distributorId && d.Category == category);
            if (distributor is null)
                return false;

            Distributors.Remove(distributor);
            return true;
        }
    }
}
