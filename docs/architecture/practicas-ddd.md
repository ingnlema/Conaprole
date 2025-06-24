# 🧠 Prácticas Aplicadas Alineadas con Domain-Driven Design (DDD)

## 🎯 Introducción al Enfoque DDD

### ¿Qué es Domain-Driven Design?

Domain-Driven Design (DDD) es un enfoque de desarrollo de software que se centra en el modelado de software para que coincida con el dominio según la comprensión de los expertos del dominio. En el proyecto **API Core Conaprole**, DDD se aplicó como filosofía de diseño fundamental para crear un sistema que representa fielmente las reglas de negocio del dominio lácteo.

### ¿Por qué se aplicó DDD en este proyecto?

El sistema de gestión de pedidos lácteos de Conaprole presenta una complejidad de dominio significativa:
- **Reglas de negocio complejas**: Validaciones de productos, cálculos de precios, estados de pedidos
- **Múltiples agregados**: Pedidos, usuarios, productos, distribuidores, puntos de venta
- **Lógica de dominio rica**: Operaciones monetarias, validaciones de cantidades, gestión de direcciones

### Organización del Código en Torno al Dominio

El código se estructura siguiendo los principios de **Clean Architecture** con DDD:

```
src/
├── Conaprole.Orders.Domain/        # ❤️ Núcleo del Dominio
│   ├── Orders/                     # Agregado Order
│   ├── Users/                      # Agregado User  
│   ├── Products/                   # Agregado Product
│   ├── Shared/                     # Value Objects compartidos
│   └── Abstractions/               # Contratos base
├── Conaprole.Orders.Application/   # 🔧 Casos de Uso
└── Conaprole.Orders.Infrastructure/ # 🔌 Servicios Externos
```

**Principio clave**: El dominio es independiente de frameworks, bases de datos y servicios externos.

---

## 📚 Prácticas Aplicadas

### 1. Uso de Entidades con Identidad Persistente

Las **entidades** en DDD son objetos que tienen una identidad única que perdura a lo largo del tiempo. En el proyecto, todas las entidades heredan de la clase base `Entity`:

```csharp
// src/Conaprole.Orders.Domain/Abstractions/Entity.cs
public abstract class Entity
{
    public Guid Id { get; init; }
    
    private readonly List<IDomainEvent> _domainEvents = new();
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

**Ejemplo de Entidad - Order:**

```csharp
// src/Conaprole.Orders.Domain/Orders/Order.cs
public class Order : Entity, IAggregateRoot
{
    public Guid DistributorId { get; private set; }
    public Guid PointOfSaleId { get; private set; }
    public Address DeliveryAddress { get; private set; }
    public Status Status { get; private set; }
    public Money Price { get; private set; }
    
    // Comportamiento encapsulado
    public void AddOrderLine(OrderLine orderLine)
    {
        if (_orderLines.Any(l => l.Product.Id == orderLine.Product.Id))
            throw new DomainException("Product already added to order.");
        
        _orderLines.Add(orderLine);
        Price += orderLine.SubTotal;
        RaiseDomainEvent(new OrderLineAddedEvent(Id, orderLine.Id));
    }
}
```

**Características de las Entidades:**
- ✅ **Identidad única**: Cada entidad tiene un `Guid Id` único
- ✅ **Comportamiento encapsulado**: Métodos que modifican el estado internamente
- ✅ **Invariantes protegidas**: Validaciones que mantienen la consistencia
- ✅ **Domain Events**: Comunicación desacoplada con otros agregados

### 2. Uso de Value Objects

Los **Value Objects** encapsulan conceptos del dominio que se definen por sus valores, no por su identidad. Son inmutables y contienen lógica de negocio.

#### Ejemplo Concreto: Value Object `Money`

```csharp
// src/Conaprole.Orders.Domain/Shared/Money.cs
public record Money(decimal Amount, Currency Currency)
{
    // Validación de reglas de negocio
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
            throw new InvalidOperationException("Currencies have to be equal");
        
        return new Money(first.Amount + second.Amount, first.Currency);
    }
    
    public static Money operator *(Money money, Quantity quantity)
    {
        return new Money(money.Amount * quantity.Value, money.Currency);
    }
    
    public static Money operator -(Money first, Money second)
    {
        if (first.Currency != second.Currency)
            throw new InvalidOperationException("Currencies have to be equal");
        
        if (first.Amount < second.Amount)
            throw new InvalidOperationException("Invalid Money operation");
        
        return new Money(first.Amount - second.Amount, first.Currency);
    }
    
    public static Money Zero(Currency currency) => new(0, currency);
    public bool IsZero() => this == Zero(Currency);
}
```

**Controles de Validación y Comportamiento:**
- ✅ **Validación de monedas**: No se pueden sumar monedas de diferentes divisas
- ✅ **Validación de operaciones**: No se pueden realizar restas que resulten en valores negativos
- ✅ **Operadores sobrecargados**: Sintaxis natural para operaciones matemáticas
- ✅ **Métodos de utilidad**: `Zero()`, `IsZero()` para casos comunes

#### Ejemplo Concreto: Value Object `Quantity`

```csharp
// src/Conaprole.Orders.Domain/Shared/Quantity.cs
public record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        Value = value;
    }
    
    // Conversiones implícitas para facilitar el uso
    public static implicit operator int(Quantity q) => q.Value;
    public static explicit operator Quantity(int value) => new Quantity(value);
}
```

#### Ejemplo Concreto: Value Object `Address`

```csharp
// src/Conaprole.Orders.Domain/Shared/Address.cs
public record Address(string City, string Street, string ZipCode)
{
    public static Address FromString(string addressString)
    {
        if (string.IsNullOrWhiteSpace(addressString))
            return new Address("", "", "");
        
        // Lógica de parsing compleja encapsulada
        var pattern = @"Address\s*\{\s*City\s*=\s*([^,]+),\s*Street\s*=\s*([^,]+),\s*ZipCode\s*=\s*([^}]+)\s*\}";
        var match = Regex.Match(addressString, pattern);
        
        if (match.Success)
        {
            return new Address(
                match.Groups[1].Value.Trim(),
                match.Groups[2].Value.Trim(), 
                match.Groups[3].Value.Trim()
            );
        }
        
        return new Address("", "", "");
    }
}
```

#### Beneficios de Value Objects frente a Tipos Primitivos

| Aspecto | Tipo Primitivo | Value Object |
|---------|---------------|--------------|
| **Expresividad** | `decimal amount` | `Money price` |
| **Validación** | Manual en cada uso | Automática en construcción |
| **Operaciones** | Lógica dispersa | Encapsulada en el objeto |
| **Tipos de Error** | Errores de runtime | Errores de compilación |
| **Mantenibilidad** | Cambios en múltiples lugares | Cambios centralizados |

**Ejemplo de Mejora:**

```csharp
// ❌ Primitivo - Propenso a errores
public void CalculatePrice(decimal unitPrice, int quantity, string currency)
{
    if (quantity <= 0) throw new Exception("Invalid quantity");
    if (unitPrice < 0) throw new Exception("Invalid price");
    // Lógica de cálculo repetida en múltiples lugares
}

// ✅ Value Objects - Expresivo y seguro
public void CalculatePrice(Money unitPrice, Quantity quantity)
{
    // Validaciones ya realizadas en la construcción
    Money total = unitPrice * quantity; // Operación natural
}
```

### 3. Uso de Agregados y Reglas de Consistencia Local

Los **agregados** son grupos de entidades y value objects que forman una unidad de consistencia. Cada agregado tiene una **raíz de agregado** que controla el acceso.

#### Ejemplo: Agregado `Order`

```csharp
// src/Conaprole.Orders.Domain/Orders/Order.cs
public class Order : Entity, IAggregateRoot
{
    private readonly List<OrderLine> _orderLines = new();
    
    // Acceso controlado a entidades hijas
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
    
    // Reglas de consistencia local
    public void AddOrderLine(OrderLine orderLine)
    {
        // Invariante: No productos duplicados
        if (_orderLines.Any(l => l.Product.Id == orderLine.Product.Id))
            throw new DomainException("Product already added to order.");
        
        _orderLines.Add(orderLine);
        Price += orderLine.SubTotal; // Mantiene consistencia del precio
        RaiseDomainEvent(new OrderLineAddedEvent(Id, orderLine.Id));
    }
    
    public void RemoveOrderLine(Guid orderLineId)
    {
        var line = _orderLines.SingleOrDefault(l => l.Id == orderLineId)
                   ?? throw new DomainException("Order line not found.");
        
        // Invariante: No se puede eliminar la última línea
        if (_orderLines.Count <= 1)
            throw new DomainException("Cannot remove the last order line.");
        
        _orderLines.Remove(line);
        Price -= line.SubTotal; // Mantiene consistencia del precio
    }
}
```

**Reglas de Consistencia Implementadas:**
- ✅ **Integridad referencial**: Las `OrderLine` solo existen dentro de un `Order`
- ✅ **Invariantes de negocio**: No productos duplicados, mínimo una línea por pedido
- ✅ **Consistencia de precios**: El precio total siempre refleja la suma de las líneas
- ✅ **Acceso controlado**: Solo la raíz del agregado (`Order`) puede modificar las entidades hijas

### 4. Separación Clara entre Capas

El proyecto implementa una separación estricta entre las capas siguiendo Clean Architecture:

#### Capa de Dominio (Lógica de Negocio Pura)
```csharp
// src/Conaprole.Orders.Domain/Orders/Order.cs
// Solo lógica de negocio, sin dependencias externas
public class Order : Entity, IAggregateRoot
{
    public void UpdateStatus(Status newStatus, DateTime updateTime)
    {
        // Lógica pura de negocio
        Status = newStatus;
        switch (newStatus)
        {
            case Status.Confirmed:
                ConfirmedOnUtc = updateTime;
                break;
            case Status.Delivered:
                DeliveredOnUtc = updateTime;
                break;
        }
    }
}
```

#### Capa de Aplicación (Coordinación de Casos de Uso)
```csharp
// src/Conaprole.Orders.Application/Orders/Commands/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    // Orquesta la lógica de dominio
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Validaciones de entrada
        // 2. Obtener entidades del dominio
        // 3. Ejecutar lógica de dominio
        // 4. Persistir cambios
        // 5. Publicar eventos
    }
}
```

#### Capa de Infraestructura (Persistencia y Servicios Externos)
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/OrderRepository.cs
public class OrderRepository : IOrderRepository
{
    // Implementa las interfaces definidas en el dominio
    // Maneja la persistencia sin que el dominio lo sepa
}
```

**Beneficios de la Separación:**
- ✅ **Testabilidad**: Cada capa puede probarse independientemente
- ✅ **Mantenibilidad**: Cambios en una capa no afectan a las otras
- ✅ **Flexibilidad**: Se puede cambiar la base de datos sin afectar el dominio
- ✅ **Claridad**: Responsabilidades bien definidas

---

## 🎯 Beneficios del Enfoque DDD

### 1. Mayor Cohesión del Modelo

**Antes (Modelo Anémico):**
```csharp
// Entidad sin comportamiento
public class Order
{
    public Guid Id { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderLine> Lines { get; set; }
}

// Lógica dispersa en servicios
public class OrderService
{
    public void AddLineToOrder(Order order, OrderLine line)
    {
        // Lógica de negocio fuera del dominio
        if (order.Lines.Any(l => l.ProductId == line.ProductId))
            throw new Exception("Duplicate product");
        
        order.Lines.Add(line);
        order.TotalPrice += line.SubTotal;
    }
}
```

**Después (Modelo Rico con DDD):**
```csharp
// Entidad con comportamiento encapsulado
public class Order : Entity, IAggregateRoot
{
    public void AddOrderLine(OrderLine orderLine)
    {
        // Lógica de negocio encapsulada
        if (_orderLines.Any(l => l.Product.Id == orderLine.Product.Id))
            throw new DomainException("Product already added to order.");
        
        _orderLines.Add(orderLine);
        Price += orderLine.SubTotal;
        RaiseDomainEvent(new OrderLineAddedEvent(Id, orderLine.Id));
    }
}
```

### 2. Evita Duplicación de Lógica

**Validaciones Centralizadas en Value Objects:**
```csharp
// Validación una sola vez en el constructor
public record Quantity
{
    public Quantity(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        Value = value;
    }
}

// Uso en múltiples lugares sin repetir validación
var quantity1 = new Quantity(5);  // ✅ Válido
var quantity2 = new Quantity(-1); // ❌ Exception automática
```

### 3. Mejora la Testabilidad y Expresividad

**Testabilidad Mejorada:**
```csharp
// test/Conaprole.Orders.Domain.UnitTests/Shared/MoneyTests.cs
[Fact]
public void Addition_Should_Work_WithSameCurrency()
{
    // Arrange - Objetos de dominio puros
    var money1 = new Money(50m, Currency.Uyu);
    var money2 = new Money(30m, Currency.Uyu);

    // Act - Operación de dominio
    var result = money1 + money2;

    // Assert - Comportamiento esperado
    result.Amount.Should().Be(80m);
    result.Currency.Should().Be(Currency.Uyu);
}
```

**Expresividad Mejorada:**
```csharp
// Código expresivo que refleja el lenguaje del dominio
var order = Order.Create(pointOfSale, distributor, deliveryAddress);
order.AddOrderLine(OrderLine.Create(product, new Quantity(10)));
order.UpdateStatus(Status.Confirmed, DateTime.UtcNow);

// vs. código procedural menos expresivo
var order = new Order();
order.PointOfSaleId = pointOfSaleId;
order.Status = "Confirmed";
```

### 4. Facilita la Evolución del Sistema

**Cambios Localizados:**
- ✅ **Nuevas reglas de negocio**: Se agregan en el agregado correspondiente
- ✅ **Nuevas validaciones**: Se centralizan en value objects
- ✅ **Nuevos comportamientos**: Se encapsulan en las entidades

**Ejemplo de Evolución:**
```csharp
// Fácil agregar nueva regla de negocio
public class Order : Entity, IAggregateRoot
{
    public void AddOrderLine(OrderLine orderLine)
    {
        // Regla existente
        if (_orderLines.Any(l => l.Product.Id == orderLine.Product.Id))
            throw new DomainException("Product already added to order.");
        
        // Nueva regla agregada fácilmente
        if (_orderLines.Count >= 50)
            throw new DomainException("Maximum 50 lines per order.");
        
        _orderLines.Add(orderLine);
        Price += orderLine.SubTotal;
    }
}
```

---

## 🔗 Referencias al Código

### Clases Clave del Dominio

#### Value Objects
- [`Money`](/src/Conaprole.Orders.Domain/Shared/Money.cs) - Operaciones monetarias con validación de divisas
- [`Quantity`](/src/Conaprole.Orders.Domain/Shared/Quantity.cs) - Cantidades con validación de valores positivos
- [`Address`](/src/Conaprole.Orders.Domain/Shared/Address.cs) - Direcciones con lógica de parsing

#### Entidades y Agregados
- [`Order`](/src/Conaprole.Orders.Domain/Orders/Order.cs) - Agregado raíz para gestión de pedidos
- [`OrderLine`](/src/Conaprole.Orders.Domain/Orders/OrderLine.cs) - Entidad hija dentro del agregado Order
- [`User`](/src/Conaprole.Orders.Domain/Users/User.cs) - Agregado para gestión de usuarios
- [`Product`](/src/Conaprole.Orders.Domain/Products/Product.cs) - Agregado para gestión de productos

#### Abstracciones Base
- [`Entity`](/src/Conaprole.Orders.Domain/Abstractions/Entity.cs) - Clase base para todas las entidades
- [`IAggregateRoot`](/src/Conaprole.Orders.Domain/Abstractions/IAggregateRoot.cs) - Marcador para raíces de agregado
- [`IDomainEvent`](/src/Conaprole.Orders.Domain/Abstractions/IDomainEvent.cs) - Contrato para eventos de dominio

#### Validaciones y Excepciones
- [`DomainException`](/src/Conaprole.Orders.Domain/Exceptions/DomainException.cs) - Excepciones específicas del dominio

### Fragmentos de Código Destacados

#### DDD en Acción - Operaciones Monetarias
```csharp
// Ejemplo de uso natural de value objects
Money unitPrice = new Money(1500m, Currency.Uyu);
Quantity quantity = new Quantity(3);
Money total = unitPrice * quantity; // = 4500 UYU

// Validación automática
Money price1 = new Money(1000m, Currency.Uyu);
Money price2 = new Money(500m, Currency.Usd);
Money invalid = price1 + price2; // ❌ InvalidOperationException
```

#### DDD en Acción - Gestión de Agregados
```csharp
// Creación y manipulación de agregados
var order = new Order(/*...*/);
var orderLine = new OrderLine(product, new Quantity(5), orderId, DateTime.UtcNow);

order.AddOrderLine(orderLine);           // ✅ Válido
order.AddOrderLine(orderLine);           // ❌ DomainException: "Product already added"
order.RemoveOrderLine(orderLine.Id);     // ✅ Válido
order.RemoveOrderLine(orderLine.Id);     // ❌ DomainException: "Cannot remove last line"
```

### Tests Unitarios de Dominio
Los tests unitarios demuestran el comportamiento esperado del dominio:
- [`MoneyTests`](/test/Conaprole.Orders.Domain.UnitTests/Shared/MoneyTests.cs) - Validación de operaciones monetarias
- Tests de agregados y entidades en [`/test/Conaprole.Orders.Domain.UnitTests/`](/test/Conaprole.Orders.Domain.UnitTests/)

---

## 🏆 Conclusión

El proyecto **API Core Conaprole** demuestra una implementación sólida y práctica de Domain-Driven Design que logra:

✅ **Modelo expresivo** que refleja el lenguaje del dominio lácteo  
✅ **Encapsulación fuerte** con invariantes de dominio protegidas  
✅ **Separación clara** entre lógica de negocio y concerns técnicos  
✅ **Testabilidad alta** con objetos de dominio puros  
✅ **Mantenibilidad mejorada** con cambios localizados  
✅ **Reutilización de código** mediante value objects bien diseñados  

Esta implementación facilita la evolución del sistema manteniendo la integridad del modelo de negocio y permitiendo que el código fuente sirva como documentación viva del dominio.

---

*Documentación generada para el proyecto API Core Conaprole - Sistema de Gestión de Pedidos Lácteos*