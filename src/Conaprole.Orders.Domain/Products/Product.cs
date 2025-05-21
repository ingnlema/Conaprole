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
        Category category,
        Description description, DateTime lastUpdated) : base(id)
    {
        ExternalProductId = externalProductId;
        Name = name;
        UnitPrice = unitPrice;
        Category = category;
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
    
    public Category Category { get; private set; }
    public DateTime LastUpdated { get; private set; }

}