namespace Conaprole.Orders.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId,CancellationToken cancellationToken = default);
    void Add(Product product);
    Task<Product?> GetByExternalIdAsync(ExternalProductId externalId, CancellationToken cancellationToken = default);

}