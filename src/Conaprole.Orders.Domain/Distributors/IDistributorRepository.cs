

using Conaprole.Orders.Domain.PointsOfSale;

namespace Conaprole.Orders.Domain.Distributors;


public interface IDistributorRepository
{
    Task<Distributor?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task<IReadOnlyCollection<PointOfSale>> GetAssignedPointsOfSaleAsync(string distributorPhone, CancellationToken ct = default);
    Task AddAsync(Distributor distributor, CancellationToken ct = default);
}




