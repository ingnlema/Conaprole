namespace Conaprole.Orders.Domain.Orders;

public interface IPointOfSaleRepository
{
    Task<PointOfSale?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task<IReadOnlyCollection<Distributor>> GetDistributorsWithCategoriesAsync(string posPhoneNumber, CancellationToken ct = default);
    Task AddAsync(PointOfSale pos, CancellationToken ct = default);
}