using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Domain.PointsOfSale;

public interface IPointOfSaleRepository
{
    Task<PointOfSale?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task<IReadOnlyCollection<Distributor>> GetDistributorsWithCategoriesAsync(string posPhoneNumber, CancellationToken ct = default);
    Task AddAsync(PointOfSale pos, CancellationToken ct = default);
    Task UpdateAsync(PointOfSale pointOfSale, CancellationToken ct = default);
}