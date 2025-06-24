# 📋 Convenciones de Nomenclatura y Organización del Código

## 🎯 Objetivo

Este documento define las **convenciones de nomenclatura, estructura de carpetas y organización del código fuente** adoptadas en el desarrollo de la API Core del proyecto Conaprole. Sirve como guía para facilitar el entendimiento del sistema por parte de nuevos desarrolladores y equipos de mantenimiento.

---

## 🏗️ Arquitectura y Organización General

### Estructura de Capas (Clean Architecture)

El proyecto sigue una arquitectura limpia con 4 capas principales:

```
src/
├── Conaprole.Orders.Domain/           # 🔵 Capa de Dominio
├── Conaprole.Orders.Application/      # 🟢 Capa de Aplicación  
├── Conaprole.Orders.Infrastructure/   # 🟡 Capa de Infraestructura
└── Conaprole.Orders.Api/             # 🔴 Capa de Presentación
```

### Organización por Agregados

Cada capa se organiza por **agregados de dominio**:

- **Orders** - Agregado principal de pedidos
- **Users** - Gestión de usuarios y roles
- **Distributors** - Gestión de distribuidores
- **Products** - Catálogo de productos
- **PointsOfSale** - Puntos de venta

---

## 📝 Convenciones de Nomenclatura

### 1. Clases y Entidades

#### 1.1 Entidades de Dominio
- **Patrón:** PascalCase, nombres singulares
- **Ejemplos:**
  ```csharp
  public class Order : Entity, IAggregateRoot
  public class OrderLine : Entity
  public class Product : Entity, IAggregateRoot
  public class Distributor : Entity, IAggregateRoot
  ```

#### 1.2 Value Objects
- **Patrón:** PascalCase, representan conceptos de dominio
- **Ejemplos:**
  ```csharp
  public record Money(decimal Amount, Currency Currency)
  public record Address(string City, string Street, string ZipCode)
  public record Quantity(int Value)
  ```

#### 1.3 Clases Abstractas Base
- **Patrón:** PascalCase, nombre descriptivo
- **Ejemplos:**
  ```csharp
  public abstract class Entity
  public abstract class Result
  ```

### 2. Interfaces

#### 2.1 Prefijo "I"
- **Patrón:** Prefijo `I` + PascalCase
- **Ejemplos:**
  ```csharp
  public interface IOrderRepository
  public interface IAuthenticationService
  public interface IJwtService
  public interface IAggregateRoot
  ```

#### 2.2 Interfaces de Agregados
- **Patrón:** `I` + Nombre del agregado + `Repository`
- **Ejemplos:**
  ```csharp
  public interface IOrderRepository
  public interface IProductRepository
  public interface IDistributorRepository
  public interface IPointOfSaleRepository
  ```

### 3. Comandos y Queries (CQRS)

#### 3.1 Comandos
- **Patrón:** Verbo + Entidad + sufijo `Command`
- **Ejemplos:**
  ```csharp
  public record CreateOrderCommand : ICommand<Guid>
  public record UpdateOrderStatusCommand : ICommand
  public record AddOrderLineCommand : ICommand<Guid>
  public record RemoveOrderLineCommand : ICommand
  ```

#### 3.2 Queries
- **Patrón:** `Get` + Entidad(es) + sufijo `Query`
- **Ejemplos:**
  ```csharp
  public record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>
  public record GetOrdersQuery : IQuery<List<OrderResponse>>
  public record GetAssignedPointsOfSaleQuery : IQuery<List<PointOfSaleResponse>>
  ```

#### 3.3 Handlers
- **Patrón:** Nombre del Command/Query + sufijo `Handler`
- **Ejemplos:**
  ```csharp
  internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
  internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
  ```

#### 3.4 Validadores
- **Patrón:** Nombre del Command + sufijo `Validator`
- **Ejemplos:**
  ```csharp
  public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
  public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
  ```

### 4. DTOs (Data Transfer Objects)

#### 4.1 Request DTOs (API)
- **Patrón:** Entidad + sufijo `Request`
- **Ubicación:** `src/Conaprole.Orders.Api/Controllers/{Agregado}/Dtos/`
- **Ejemplos:**
  ```csharp
  public record CreateOrderRequest(...)
  public record OrderLineRequest(...)
  public record BulkCreateOrdersRequest(...)
  ```

#### 4.2 Response DTOs (Application)
- **Patrón:** Entidad + sufijo `Response`
- **Ubicación:** `src/Conaprole.Orders.Application/{Agregado}/{CasoDeUso}/`
- **Ejemplos:**
  ```csharp
  public record OrderResponse(...)
  public record OrderLineResponse(...)
  public record ProductResponse(...)
  public record AddressResponse(...)
  public record MoneyResponse(...)
  ```

### 5. Controladores

#### 5.1 Nombres de Controladores
- **Patrón:** Nombre plural del agregado + sufijo `Controller`
- **Ejemplos:**
  ```csharp
  [Route("api/Orders")]
  public class OrdersController : ControllerBase
  
  [Route("api/Users")]
  public class UsersController : ControllerBase
  
  [Route("api/Products")]
  public class ProductsController : ControllerBase
  ```

#### 5.2 Métodos de Controladores
- **Patrón:** Verbos HTTP estándar + nombres descriptivos
- **Ejemplos:**
  ```csharp
  [HttpPost]
  public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
  
  [HttpGet("{id}")]
  public async Task<IActionResult> GetOrder(Guid id)
  
  [HttpPut("{id}/status")]
  public async Task<IActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request)
  ```

### 6. Repositorios e Infraestructura

#### 6.1 Implementaciones de Repositorios
- **Patrón:** Nombre del agregado + sufijo `Repository`
- **Ubicación:** `src/Conaprole.Orders.Infrastructure/Repositories/`
- **Ejemplos:**
  ```csharp
  internal sealed class OrderRepository : Repository<Order>, IOrderRepository
  internal sealed class ProductRepository : Repository<Product>, IProductRepository
  ```

#### 6.2 Configuraciones de Entity Framework
- **Patrón:** Nombre de la entidad + sufijo `Configuration`
- **Ubicación:** `src/Conaprole.Orders.Infrastructure/Configuration/`
- **Ejemplos:**
  ```csharp
  internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
  internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
  ```

### 7. Excepciones y Errores

#### 7.1 Excepciones de Dominio
- **Patrón:** Nombre descriptivo + sufijo `Exception`
- **Ejemplos:**
  ```csharp
  public sealed class ConflictException : Exception
  public sealed class NotFoundException : Exception
  public sealed class ConcurrencyException : Exception
  ```

#### 7.2 Errores de Dominio
- **Patrón:** Nombre del agregado + sufijo `Errors`
- **Ejemplos:**
  ```csharp
  public static class OrderErrors
  public static class UserErrors
  public static class ProductErrors
  ```

---

## 📁 Organización de Carpetas Detallada

### 1. Capa de Dominio (`src/Conaprole.Orders.Domain/`)

```
Conaprole.Orders.Domain/
├── Abstractions/                  # Contratos base y abstracciones
│   ├── Entity.cs                 # Clase base para entidades
│   ├── IAggregateRoot.cs         # Interfaz para agregados
│   ├── IDomainEvent.cs           # Interfaz para eventos
│   ├── Result.cs                 # Patrón Result para manejo de errores
│   └── Error.cs                  # Definición de errores
├── Orders/                       # Agregado Order
│   ├── Order.cs                  # Entidad raíz del agregado
│   ├── OrderLine.cs              # Entidad relacionada
│   ├── OrderId.cs                # Value object para ID
│   ├── Status.cs                 # Enumeración de estados
│   ├── IOrderRepository.cs       # Contrato del repositorio
│   ├── OrderErrors.cs            # Errores específicos del dominio
│   └── Events/                   # Eventos de dominio
├── Users/                        # Agregado User
├── Distributors/                 # Agregado Distributor
├── Products/                     # Agregado Product
├── PointsOfSale/                 # Agregado PointOfSale
├── Shared/                       # Value Objects compartidos
│   ├── Money.cs
│   ├── Address.cs
│   ├── Currency.cs
│   └── Quantity.cs
└── Exceptions/                   # Excepciones de dominio
```

### 2. Capa de Aplicación (`src/Conaprole.Orders.Application/`)

```
Conaprole.Orders.Application/
├── Abstractions/                 # Contratos de aplicación
│   ├── Messaging/               # Interfaces CQRS
│   │   ├── ICommand.cs
│   │   ├── IQuery.cs
│   │   ├── ICommandHandler.cs
│   │   └── IQueryHandler.cs
│   ├── Authentication/          # Contratos de autenticación
│   ├── Behaviors/              # Behaviors de MediatR
│   └── Clock/                  # Servicios de tiempo
├── Orders/                      # Casos de uso de Orders
│   ├── CreateOrder/            # Caso de uso específico
│   │   ├── CreateOrderCommand.cs
│   │   ├── CreateOrderCommandHandler.cs
│   │   ├── CreateOrderCommandValidator.cs
│   │   └── CreateOrderLineCommand.cs
│   ├── GetOrder/               # Otro caso de uso
│   │   ├── GetOrderQuery.cs
│   │   ├── GetOrderQueryHandler.cs
│   │   ├── OrderResponse.cs
│   │   └── OrderLineResponse.cs
│   ├── UpdateOrderStatus/
│   ├── AddOrderLine/
│   └── RemoveOrderLine/
├── Users/                      # Casos de uso de Users
├── Distributors/              # Casos de uso de Distributors
├── Products/                  # Casos de uso de Products
├── PointsOfSale/             # Casos de uso de PointsOfSale
└── Exceptions/               # Excepciones de aplicación
```

### 3. Capa de Infraestructura (`src/Conaprole.Orders.Infrastructure/`)

```
Conaprole.Orders.Infrastructure/
├── Data/                        # Configuración de datos
├── Repositories/               # Implementaciones de repositorios
│   ├── Repository.cs           # Repositorio base
│   ├── OrderRepository.cs
│   ├── ProductRepository.cs
│   └── UserRepository.cs
├── Configuration/              # Configuraciones EF Core
│   ├── OrderConfiguration.cs
│   ├── OrderLineConfiguration.cs
│   └── ProductConfiguration.cs
├── Authentication/             # Servicios de autenticación
├── Authorization/              # Servicios de autorización
├── Clock/                     # Implementaciones de tiempo
├── Migrations/                # Migraciones de base de datos
└── ApplicationDbContext.cs    # Contexto de Entity Framework
```

### 4. Capa de API (`src/Conaprole.Orders.Api/`)

```
Conaprole.Orders.Api/
├── Controllers/                # Controladores REST
│   ├── Orders/                # Controlador de Orders
│   │   ├── OrdersController.cs
│   │   ├── Dtos/              # DTOs específicos del controlador
│   │   │   ├── CreateOrderRequest.cs
│   │   │   ├── OrderLineRequest.cs
│   │   │   └── Examples/      # Ejemplos para Swagger
│   │   └── MappingExtensions.cs # Extensiones de mapeo
│   ├── Users/
│   ├── Products/
│   └── Distributors/
├── Middelware/                # Middleware personalizado
├── Extensions/                # Métodos de extensión
└── Program.cs                # Punto de entrada de la aplicación
```

---

## 📋 Patrones y Convenciones Específicas

### 1. Patrones de Archivos por Caso de Uso

**Para Comandos (operaciones de escritura):**
```
CreateOrder/
├── CreateOrderCommand.cs          # El comando
├── CreateOrderCommandHandler.cs   # Manejador del comando
├── CreateOrderCommandValidator.cs # Validador
└── CreateOrderLineCommand.cs      # Comando anidado si es necesario
```

**Para Queries (operaciones de lectura):**
```
GetOrder/
├── GetOrderQuery.cs              # La query
├── GetOrderQueryHandler.cs       # Manejador de la query
├── OrderResponse.cs              # DTO de respuesta
├── OrderLineResponse.cs          # DTOs relacionados
└── ProductResponse.cs            # DTOs relacionados
```

### 2. Namespaces

Los namespaces siguen la estructura de carpetas:

```csharp
// Dominio
namespace Conaprole.Orders.Domain.Orders;
namespace Conaprole.Orders.Domain.Shared;

// Aplicación
namespace Conaprole.Orders.Application.Orders.CreateOrder;
namespace Conaprole.Orders.Application.Abstractions.Messaging;

// Infraestructura
namespace Conaprole.Orders.Infrastructure.Repositories;
namespace Conaprole.Orders.Infrastructure.Configuration;

// API
namespace Conaprole.Orders.Api.Controllers.Orders;
```

### 3. Modificadores de Acceso

- **Entidades de dominio:** `public` (necesarias para EF Core)
- **Handlers:** `internal sealed` (implementación interna)
- **Repositorios:** `internal sealed` (implementación interna)
- **Controladores:** `public` (endpoints públicos)
- **DTOs:** `public` (serializables)

---

## 🔗 Ejemplos Concretos del Proyecto

### Ejemplo 1: Flujo Completo de Creación de Orden

**1. Request DTO (API Layer):**
```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/Dtos/CreateOrderRequest.cs
namespace Conaprole.Orders.Api.Controllers.Orders;

public record CreateOrderRequest(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<OrderLineRequest> OrderLines);
```

**2. Command (Application Layer):**
```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommand.cs
namespace Conaprole.Orders.Application.Orders.CreateOrder;

public record CreateOrderCommand(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<CreateOrderLineCommand> OrderLines) : ICommand<Guid>;
```

**3. Handler (Application Layer):**
```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommandHandler.cs
namespace Conaprole.Orders.Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Implementación...
    }
}
```

**4. Entity (Domain Layer):**
```csharp
// src/Conaprole.Orders.Domain/Orders/Order.cs
namespace Conaprole.Orders.Domain.Orders;

public class Order : Entity, IAggregateRoot
{
    // Propiedades y métodos...
}
```

### Ejemplo 2: Consulta de Orden

**1. Query:**
```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/GetOrderQuery.cs
public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>;
```

**2. Handler:**
```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/GetOrderQueryHandler.cs
internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
```

**3. Response DTO:**
```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/OrderResponse.cs
public sealed record OrderResponse(
    Guid Id,
    string DistributorName,
    string PointOfSaleName,
    AddressResponse DeliveryAddress,
    string Status,
    DateTime CreatedOnUtc,
    MoneyResponse Price,
    List<OrderLineResponse> OrderLines);
```

---

## ✅ Buenas Prácticas y Recomendaciones

### 1. Consistencia en Nombres
- **Usar siempre el mismo patrón** para elementos similares
- **Evitar abreviaciones** no estándar
- **Mantener nombres descriptivos** pero concisos

### 2. Organización de Archivos
- **Un archivo por clase/record** público
- **Agrupar archivos relacionados** en carpetas específicas
- **Mantener la separación por capas** estrictamente

### 3. Interfaces y Abstracciones
- **Definir interfaces en la capa más interna** que las necesite
- **Usar segregación de interfaces** (ISP)
- **Prefijo "I" consistente** para todas las interfaces

### 4. Commands y Queries
- **Separar claramente** operaciones de lectura y escritura
- **Una responsabilidad por comando/query**
- **Validadores específicos** para cada comando

### 5. DTOs y Mapping
- **DTOs inmutables** usando records
- **Mapping explícito** entre capas
- **Validación en los DTOs** de entrada

### 6. Manejo de Errores
- **Patrón Result** para operaciones que pueden fallar
- **Errores específicos** por dominio
- **Excepciones solo para casos excepcionales**

---

## 🔍 Referencias Útiles

### Archivos Clave para Referencia
- **Estructura general:** `docs/architecture/clean-architecture.md`
- **Implementación CQRS:** `docs/architecture/cqrs-mediator.md`
- **Principios SOLID:** `docs/architecture/solid-y-dip.md`
- **Diseño de API:** `docs/architecture/api-design.md`

### Ejemplos de Implementación
- **Entidad completa:** `src/Conaprole.Orders.Domain/Orders/Order.cs`
- **Caso de uso completo:** `src/Conaprole.Orders.Application/Orders/CreateOrder/`
- **Controlador completo:** `src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs`
- **Repositorio completo:** `src/Conaprole.Orders.Infrastructure/Repositories/OrderRepository.cs`

---

*Este documento es una guía viva que debe actualizarse conforme evoluciona el proyecto. Para consultas específicas o propuestas de mejora, referirse a la documentación arquitectónica complementaria.*