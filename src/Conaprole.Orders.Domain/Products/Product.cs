using Conaprole.Orders.Domain.Shared;
namespace Conaprole.Orders.Domain.Products;
using Abstractions;
public class Product : Entity
{
    public Product(
        Guid id, 
        ExternalProductId externalProductId, 
        Name name,
        Money unitPrice,
        Description description, DateTime lastUpdated) : base(id)
    {
        ExternalProductId = externalProductId;
        Name = name;
        UnitPrice = unitPrice;
        Description = description;
        LastUpdated = lastUpdated;
    }
    
    private Product()
    {
        
    }
    
    public ExternalProductId ExternalProductId { get; set; }
    public Name Name { get; private set; }
    public Money UnitPrice { get; private set; }
    public Description Description { get; private set; }
    public List<Category> Categories { get; set; } = new();
    public DateTime LastUpdated { get; private set; }

}