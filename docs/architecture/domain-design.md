# 🎯 Domain-Driven Design - Modelo de Dominio API Core Conaprole

## Introducción

La API Core de Conaprole Orders implementa **Domain-Driven Design (DDD)** como enfoque de modelado, centrándose en el conocimiento del dominio de negocio y la colaboración entre expertos técnicos y del dominio. El modelo representa fielmente las reglas de negocio del sistema de gestión de pedidos lácteos.

## Fundamentos DDD Implementados

### 1. Ubiquitous Language (Lenguaje Ubicuo)
- **Order (Pedido)**: Solicitud de productos realizada por un punto de venta
- **Distributor (Distribuidor)**: Entidad encargada de entregar productos
- **Point of Sale (Punto de Venta)**: Cliente que realiza pedidos
- **Product (Producto)**: Artículo lácteo disponible para pedido
- **Order Line (Línea de Pedido)**: Detalle específico de un producto en un pedido

### 2. Strategic Design Patterns
- **Bounded Context**: Sistema de Orders como contexto delimitado
- **Domain Model**: Entidades ricas con comportamiento
- **Aggregate Pattern**: Consistencia transaccional

## Agregados (Aggregates)

### 🏷️ Agregado Order

**Aggregate Root**: `Order`

```csharp
// src/Conaprole.Orders.Domain/Orders/Order.cs
public class Order : Entity, IAggregateRoot
{
    private readonly List<OrderLine> _orderLines = new();
    
    // Propiedades principales
    public Guid DistributorId { get; private set; }
    public Guid PointOfSaleId { get; private set; }
    public Address DeliveryAddress { get; private set; }
    public Status Status { get; private set; }
    public Money Price { get; private set; }
    
    // Entidades hijas
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
    
    // Comportamientos de dominio
    public void AddOrderLine(OrderLine orderLine)
    {
        _orderLines.Add(orderLine);
        RecalculatePrice();
        RaiseDomainEvent(new OrderLineAddedDomainEvent(Id, orderLine.Id));
    }
    
    public void UpdateStatus(Status newStatus)
    {
        if (!CanChangeStatusTo(newStatus))
            throw new DomainException("Invalid status transition");
            
        Status = newStatus;
        RaiseDomainEvent(new OrderStatusChangedDomainEvent(Id, Status));
    }
}
```

**Entidades Hijas**:
```csharp
// src/Conaprole.Orders.Domain/Orders/OrderLine.cs
public class OrderLine : Entity
{
    public Quantity Quantity { get; private set; }
    public Money SubTotal { get; private set; }
    public Product Product { get; private set; }
    public OrderId OrderId { get; private set; }
    
    public void UpdateQuantity(Quantity newQuantity)
    {
        if (newQuantity.Value <= 0)
            throw new DomainException("Quantity must be positive");
            
        Quantity = newQuantity;
        SubTotal = Product.UnitPrice * newQuantity;
    }
}
```

### 👤 Agregado User

**Aggregate Root**: `User`

```csharp
// src/Conaprole.Orders.Domain/Users/User.cs
public sealed class User : Entity
{
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    public Email Email { get; private set; }
    public Guid IdentityId { get; private set; }
    
    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    
    public void AssignRole(Role role)
    {
        if (_roles.Contains(role))
            return;
            
        _roles.Add(role);
        RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, role.Id));
    }
    
    public bool HasPermission(string permission)
    {
        return _roles
            .SelectMany(r => r.Permissions)
            .Any(p => p.Name == permission);
    }
}
```

### 🚚 Agregado Distributor

**Aggregate Root**: `Distributor`

```csharp
// src/Conaprole.Orders.Domain/Distributors/Distributor.cs
public sealed class Distributor : Entity
{
    public Name Name { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    
    private readonly List<Category> _categories = new();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();
    
    public void AddCategory(Category category)
    {
        if (_categories.Contains(category))
            return;
            
        _categories.Add(category);
        RaiseDomainEvent(new DistributorCategoryAddedDomainEvent(Id, category));
    }
    
    public bool CanDistributeProduct(Product product)
    {
        return _categories.Contains(product.Category);
    }
}
```

### 🏪 Agregado PointOfSale

**Aggregate Root**: `PointOfSale`

```csharp
// src/Conaprole.Orders.Domain/PointsOfSale/PointOfSale.cs
public sealed class PointOfSale : Entity
{
    public Name Name { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    
    public static PointOfSale Create(Name name, PhoneNumber phoneNumber, Address address)
    {
        var pointOfSale = new PointOfSale(Guid.NewGuid(), name, phoneNumber, address);
        pointOfSale.RaiseDomainEvent(new PointOfSaleCreatedDomainEvent(pointOfSale.Id));
        return pointOfSale;
    }
}
```

### 📦 Agregado Product

**Aggregate Root**: `Product`

```csharp
// src/Conaprole.Orders.Domain/Products/Product.cs
public sealed class Product : Entity
{
    public Name Name { get; private set; }
    public ExternalProductId ExternalId { get; private set; }
    public Money UnitPrice { get; private set; }
    public Category Category { get; private set; }
    
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount <= 0)
            throw new DomainException("Price must be positive");
            
        UnitPrice = newPrice;
        RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, newPrice));
    }
}
```

## Value Objects

### 💰 Money
```csharp
// src/Conaprole.Orders.Domain/Shared/Money.cs
public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0, currency);
    
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
            throw new InvalidOperationException("Currencies must be equal");
        
        return new Money(first.Amount + second.Amount, first.Currency);
    }
    
    public static Money operator *(Money money, Quantity quantity)
    {
        return new Money(money.Amount * quantity.Value, money.Currency);
    }
}
```

### 🏠 Address
```csharp
// src/Conaprole.Orders.Domain/Shared/Address.cs
public record Address(string City, string Street, string ZipCode)
{
    public static Address Create(string city, string street, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty");
            
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street cannot be empty");
            
        return new Address(city, street, zipCode);
    }
}
```

### 📊 Quantity
```csharp
// src/Conaprole.Orders.Domain/Shared/Quantity.cs
public record Quantity(int Value)
{
    public Quantity(int value) : this(value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be positive");
    }
    
    public static implicit operator int(Quantity quantity) => quantity.Value;
    public static implicit operator Quantity(int value) => new(value);
}
```

## Domain Events

### 📢 Order Events
```csharp
// src/Conaprole.Orders.Domain/Orders/Events/OrderCreatedDomainEvent.cs
public sealed record OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent;

// src/Conaprole.Orders.Domain/Orders/Events/OrderStatusChangedDomainEvent.cs
public sealed record OrderStatusChangedDomainEvent(Guid OrderId, Status NewStatus) : IDomainEvent;

// src/Conaprole.Orders.Domain/Orders/Events/OrderLineAddedDomainEvent.cs
public sealed record OrderLineAddedDomainEvent(Guid OrderId, Guid OrderLineId) : IDomainEvent;
```

### Event Handlers (en Application Layer)
```csharp
// src/Conaprole.Orders.Application/Orders/Events/OrderCreatedDomainEventHandler.cs
internal sealed class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Lógica de manejo del evento (notificaciones, logs, etc.)
    }
}
```

## Repository Pattern

### Interfaces en Domain
```csharp
// src/Conaprole.Orders.Domain/Orders/IOrderRepository.cs
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByDistributorIdAsync(Guid distributorId, CancellationToken cancellationToken = default);
    void Add(Order order);
    void Update(Order order);
    void Remove(Order order);
}
```

### Implementaciones en Infrastructure
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/OrderRepository.cs
internal sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Order>()
            .Include(o => o.OrderLines)
            .ThenInclude(ol => ol.Product)
            .Include(o => o.Distributor)
            .Include(o => o.PointOfSale)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
```

## Domain Services

### Business Logic Compleja
```csharp
// src/Conaprole.Orders.Domain/Orders/OrderDomainService.cs
public class OrderDomainService
{
    public bool CanDistributorFulfillOrder(Distributor distributor, Order order)
    {
        return order.OrderLines.All(line => 
            distributor.CanDistributeProduct(line.Product));
    }
    
    public Money CalculateOrderTotal(Order order)
    {
        if (!order.OrderLines.Any())
            return Money.Zero(Currency.UYU);
            
        return order.OrderLines
            .Select(line => line.SubTotal)
            .Aggregate((total, lineTotal) => total + lineTotal);
    }
}
```

## Specification Pattern

### Especificaciones de Dominio
```csharp
// src/Conaprole.Orders.Domain/Orders/Specifications/OrdersByDateRangeSpecification.cs
public class OrdersByDateRangeSpecification : Specification<Order>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    
    public OrdersByDateRangeSpecification(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }
    
    public override Expression<Func<Order, bool>> ToExpression()
    {
        return order => order.CreatedOnUtc >= _startDate && order.CreatedOnUtc <= _endDate;
    }
}
```

## Errores de Dominio

### Domain Errors
```csharp
// src/Conaprole.Orders.Domain/Orders/OrderErrors.cs
public static class OrderErrors
{
    public static readonly Error NotFound = new(
        "Order.NotFound",
        "The order with the specified identifier was not found");
        
    public static readonly Error InvalidStatus = new(
        "Order.InvalidStatus",
        "The order status transition is not valid");
        
    public static readonly Error EmptyOrderLines = new(
        "Order.EmptyOrderLines",
        "An order must have at least one order line");
}
```

## Invariantes de Dominio

### Reglas de Negocio Implementadas

1. **Order Invariants**:
   - Un pedido debe tener al menos una línea de pedido
   - El precio total debe ser la suma de todas las líneas
   - Solo se permiten transiciones de estado válidas

2. **OrderLine Invariants**:
   - La cantidad debe ser mayor a cero
   - El subtotal debe calcularse automáticamente

3. **User Invariants**:
   - Un usuario no puede tener roles duplicados
   - El email debe ser único en el sistema

4. **Distributor Invariants**:
   - Solo puede distribuir productos de categorías asignadas
   - El teléfono debe ser único

## Modelo de Dominio - Diagrama

```mermaid
classDiagram
    class Order {
        +Guid Id
        +Address DeliveryAddress
        +Status Status
        +Money Price
        +AddOrderLine(OrderLine)
        +UpdateStatus(Status)
    }
    
    class OrderLine {
        +Quantity Quantity
        +Money SubTotal
        +UpdateQuantity(Quantity)
    }
    
    class User {
        +FirstName FirstName
        +LastName LastName
        +Email Email
        +AssignRole(Role)
        +HasPermission(string)
    }
    
    class Distributor {
        +Name Name
        +PhoneNumber PhoneNumber
        +Address Address
        +AddCategory(Category)
        +CanDistributeProduct(Product)
    }
    
    class Product {
        +Name Name
        +Money UnitPrice
        +Category Category
        +UpdatePrice(Money)
    }
    
    Order ||--o{ OrderLine : contains
    Order }o--|| Distributor : assigned_to
    Order }o--|| PointOfSale : requested_by
    OrderLine }o--|| Product : for
    User ||--o{ Role : has
    Distributor ||--o{ Category : distributes
```

## Conclusión

El modelo de dominio de la API Core Conaprole demuestra una implementación sólida de DDD con:

- **Agregados bien definidos** con responsabilidades claras
- **Value Objects** que encapsulan conceptos de negocio
- **Domain Events** para comunicación desacoplada
- **Repository Pattern** para abstracción de persistencia
- **Invariantes de dominio** que garantizan consistencia
- **Lenguaje ubicuo** consistente en todo el código

Esta implementación facilita la evolución del sistema manteniendo la integridad del modelo de negocio.

---

*Próximo: [CQRS y MediatR](./cqrs-mediator.md) - Implementación de comandos y queries*