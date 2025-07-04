---
title: "Stack Tecnológico y Convenciones - Conaprole Orders"
description: "Descripción completa del stack tecnológico, frameworks, librerías y convenciones de desarrollo utilizadas en Conaprole Orders API"
last_verified_sha: "bbed9c1ad056ddda4c3b5f646638bc9f77b4c31d"
---

# 🛠️ Stack Tecnológico y Convenciones - Conaprole Orders

## Purpose

Este documento describe el **stack tecnológico completo**, frameworks, librerías, herramientas y convenciones de desarrollo adoptadas en el proyecto Conaprole Orders API, proporcionando una visión integral de las tecnologías y estándares utilizados.

## Audience

- **Desarrolladores** - Comprensión del stack técnico y convenciones
- **Arquitectos** - Decisiones tecnológicas y dependencias
- **DevOps/SysAdmins** - Tecnologías para deployment y operaciones
- **Personal Académico** - Análisis tecnológico para documentación de tesis

## Prerequisites

- Conocimiento básico de .NET y ecosistema Microsoft
- Familiaridad con desarrollo web y APIs REST
- Comprensión de arquitecturas empresariales

## 🎯 Stack Tecnológico Principal

### Plataforma y Framework Base

| Tecnología | Versión | Propósito | Justificación |
|------------|---------|-----------|---------------|
| **.NET** | 8.0 | Framework principal | LTS, performance, funcionalidades modernas |
| **C#** | 12.0 | Lenguaje de programación | Type safety, productividad, ecosistema |
| **ASP.NET Core** | 8.0 | Framework web | Alto rendimiento, cross-platform |

### Arquitectura y Patrones

| Patrón/Framework | Implementación | Propósito |
|------------------|----------------|-----------|
| **Clean Architecture** | 4 capas separadas | Mantenibilidad, testabilidad, separación de responsabilidades |
| **Domain-Driven Design** | Agregados, Value Objects | Modelado del dominio de negocio |
| **CQRS** | MediatR | Separación comando/query, escalabilidad |
| **Repository Pattern** | Abstracciones + EF Core | Desacoplamiento de persistencia |

## 📦 Dependencias y Librerías

### Capa de Aplicación

```xml
<PackageReference Include="MediatR" Version="12.4.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
<PackageReference Include="Dapper" Version="2.1.66" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.2" />
<PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="9.0.0" />
```

#### MediatR - Implementación CQRS

```csharp
// Configuración en Program.cs
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// Ejemplo de Command
public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderLineRequest> OrderLines
) : IRequest<OrderResponse>;

// Handler correspondiente
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Lógica de creación de orden
    }
}
```

#### FluentValidation - Validación de Comandos

```csharp
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");
            
        RuleFor(x => x.OrderLines)
            .NotEmpty()
            .WithMessage("At least one order line is required")
            .Must(lines => lines.Count <= 50)
            .WithMessage("Maximum 50 order lines allowed");
    }
}
```

### Capa de Infraestructura

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
<PackageReference Include="EFCore.NamingConventions" Version="8.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.14" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
```

#### Entity Framework Core - ORM

```csharp
// Configuración de contexto
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention(); // Para PostgreSQL
});

// Configuración de entidad
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => new OrderId(value));
            
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("total_amount");
            money.Property(m => m.Currency).HasColumnName("currency")
                .HasConversion<string>();
        });
    }
}
```

#### PostgreSQL - Base de Datos

**Características utilizadas:**
- **JSONB** para datos semi-estructurados
- **UUID** como identificadores primarios
- **Índices** para optimización de consultas
- **Transacciones** para integridad de datos

```sql
-- Ejemplo de tabla generada
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);
```

### Capa de API

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore.Filters" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Bogus" Version="35.6.2" />
```

#### Swagger/OpenAPI - Documentación

```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Conaprole Orders API", 
        Version = "v1",
        Description = "API para gestión de pedidos de productos lácteos"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.EnableAnnotations();
});
```

#### Serilog - Logging Estructurado

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/conaprole-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();
```

## 🔐 Seguridad y Autenticación

### Keycloak - Identity Provider

| Componente | Tecnología | Configuración |
|------------|------------|---------------|
| **Servidor de Identidad** | Keycloak 24.x | Realm: `Conaprole` |
| **Protocolo** | OpenID Connect | JWT Bearer tokens |
| **Algoritmo** | RS256 | Verificación asimétrica |

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Keycloak:Authority"];
        options.Audience = configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = environment.IsProduction();
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
```

### Authorization System

```csharp
// Atributo personalizado para permisos
public class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permission;
    
    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Lógica de verificación de permisos
    }
}

// Uso en controllers
[HttpGet]
[HasPermission("orders:read")]
public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
{
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

## 🧪 Testing Stack

### Frameworks de Testing

| Framework | Versión | Propósito | Capa |
|-----------|---------|-----------|------|
| **xUnit** | 2.4.2 | Testing framework principal | Todas |
| **FluentAssertions** | 6.12.0 | Assertions expresivas | Todas |
| **Testcontainers** | 3.9.0 | Containers para integration tests | Integration |
| **Bogus** | 35.6.2 | Generación de datos de prueba | Todas |
| **Moq** | 4.20.69 | Mocking framework | Unit Tests |

### Test Categories

#### Unit Tests
```csharp
public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidData_ShouldSucceed()
    {
        // Arrange
        var customerId = new UserId(Guid.NewGuid());
        var orderLines = new List<OrderLine>
        {
            OrderLine.Create(
                new ProductId(Guid.NewGuid()),
                new Quantity(2, "units"),
                new Money(100, Currency.USD)
            )
        };

        // Act
        var order = Order.Create(customerId, orderLines);

        // Assert
        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.TotalAmount.Should().Be(new Money(200, Currency.USD));
    }
}
```

#### Integration Tests
```csharp
public class OrdersControllerTests : BaseIntegrationTest
{
    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerId: _testUser.Id,
            OrderLines: new List<OrderLineRequest>
            {
                new(ProductId: _testProduct.Id, Quantity: 2, UnitPrice: 10.00m)
            }
        );

        // Act
        var response = await AuthenticatedRequest(
            HttpMethod.Post,
            "/api/orders",
            command,
            "orders:write"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## 🔧 Herramientas de Desarrollo

### Desarrollo Local

| Herramienta | Propósito | Configuración |
|-------------|-----------|---------------|
| **Docker** | Containerización | PostgreSQL, Keycloak |
| **Docker Compose** | Orquestación local | `docker-compose.yaml` |
| **Makefile** | Automatización | Comandos de desarrollo |

```yaml
# docker-compose.yaml
version: '3.8'
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: conaprole_orders
      POSTGRES_USER: conaprole
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
    
  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    ports:
      - "8080:8080"
    command: start-dev
```

### Quality Assurance

| Herramienta | Propósito | Configuración |
|-------------|-----------|---------------|
| **markdownlint** | Validación de documentación | `.markdownlint.json` |
| **dotnet format** | Formateo de código | Integrado en .NET CLI |
| **SonarQube** | Análisis de calidad | CI/CD pipeline |

```json
// .markdownlint.json
{
  "MD013": { "line_length": 120 },
  "MD033": false,
  "MD041": false
}
```

## 📋 Convenciones de Desarrollo

### Naming Conventions

#### Clases y Entidades
```csharp
// ✅ Correcto
public class Order : Entity, IAggregateRoot
public class OrderLine : Entity
public class CreateOrderCommand : IRequest<OrderResponse>
public class OrderCreatedEvent : DomainEvent

// ❌ Incorrecto
public class order
public class orderLine
public class Order_Entity
```

#### Métodos y Propiedades
```csharp
// ✅ Correcto
public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
public Money TotalAmount { get; private set; }
public bool IsActive { get; private set; }

// ❌ Incorrecto
public async Task<Order> createOrder(CreateOrderRequest request)
public Money totalAmount { get; private set; }
```

#### Variables y Parámetros
```csharp
// ✅ Correcto
var orderId = new OrderId(Guid.NewGuid());
var customerId = request.CustomerId;
string customerName = user.FullName;

// ❌ Incorrecto
var OrderId = new OrderId(Guid.NewGuid());
var customer_id = request.CustomerId;
```

### Folder Structure Conventions

```
src/
├── Conaprole.Orders.Domain/
│   ├── Orders/               # Agregado
│   │   ├── Order.cs         # Entidad raíz
│   │   ├── OrderLine.cs     # Entidad hija
│   │   ├── OrderId.cs       # Value Object ID
│   │   └── Events/          # Domain Events
│   ├── Users/
│   ├── Common/              # Elementos compartidos
│   └── Exceptions/          # Excepciones de dominio
├── Conaprole.Orders.Application/
│   ├── Orders/
│   │   ├── Commands/
│   │   │   ├── CreateOrder/
│   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   │   └── CreateOrderCommandValidator.cs
│   │   │   └── UpdateOrder/
│   │   └── Queries/
│   └── Common/
│       ├── Behaviors/       # Pipeline behaviors
│       ├── Exceptions/      # Application exceptions
│       └── Mappings/        # AutoMapper profiles
```

### Code Style Guidelines

#### Exception Handling
```csharp
// ✅ Uso correcto de excepciones específicas
public class Order : Entity
{
    public void AddOrderLine(OrderLine orderLine)
    {
        if (orderLine == null)
            throw new ArgumentNullException(nameof(orderLine));
            
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a confirmed order");
            
        _orderLines.Add(orderLine);
    }
}
```

#### Async/Await Patterns
```csharp
// ✅ Patrón async correcto
public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
{
    var customer = await _userRepository.GetByIdAsync(request.CustomerId);
    if (customer == null)
        throw new NotFoundException("Customer", request.CustomerId);
    
    var order = Order.Create(customer.Id, request.OrderLines);
    await _orderRepository.AddAsync(order);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return _mapper.Map<OrderResponse>(order);
}
```

## 🚀 Performance Considerations

### Database Optimizations

1. **Índices estratégicos** en columnas de búsqueda frecuente
2. **Lazy loading** deshabilitado por defecto
3. **Connection pooling** para optimizar conexiones
4. **Read replicas** para consultas de solo lectura

### Memory Management

1. **Disposable pattern** para recursos
2. **Object pooling** para objetos costosos
3. **Streaming** para operaciones con grandes volúmenes de datos

### Caching Strategy

```csharp
services.AddMemoryCache();
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});
```


## Referencias Técnicas

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Entity Framework Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

*Last verified: 2025-01-02 - Commit: bbed9c1ad056ddda4c3b5f646638bc9f77b4c31d*
