# 🌐 Diseño de API - Patrones de Controllers y Endpoints

## Introducción

La API Core de Conaprole Orders implementa una **API REST** siguiendo las mejores prácticas de diseño, 
con endpoints consistentes, documentación automática con **Swagger**, y patrones de respuesta uniformes. 
El diseño se basa en los principios **RESTful** y utiliza **ASP.NET Core** como framework web.

## Estructura de Controllers

### 📁 Organización por Dominio

```text
src/Conaprole.Orders.Api/Controllers/
├── Orders/                    # Gestión de pedidos
│   ├── OrdersController.cs
│   ├── Dtos/                 # DTOs de entrada/salida
│   └── Examples/             # Ejemplos para Swagger
├── Users/                    # Gestión de usuarios
├── Products/                 # Gestión de productos
├── Distributors/             # Gestión de distribuidores
└── PointsOfSale/            # Gestión de puntos de venta
```

### 🎯 Controlador Base Pattern

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets a specific order by its ID.
    /// </summary>
    [SwaggerOperation(Summary = "Get order by ID", Description = "Returns a specific order with all details and lines.")]
    [HttpGet("{id}")]
    [HasPermission("orders:read")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    [SwaggerOperation(Summary = "Create order", Description = "Creates a new order with address, distributor, POS and order lines.")]
    [HttpPost]
    [HasPermission("orders:write")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request, 
        CancellationToken cancellationToken)
    {
        var command = MapToCommand(request);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
```

## Diseño de Endpoints

### 🛣️ Convenciones REST

| HTTP Method | Endpoint Pattern | Descripción | Ejemplo |
|-------------|------------------|-------------|---------|
| GET | `/api/{resource}` | Listar recursos | `GET /api/orders` |
| GET | `/api/{resource}/{id}` | Obtener recurso específico | `GET /api/orders/{id}` |
| POST | `/api/{resource}` | Crear nuevo recurso | `POST /api/orders` |
| PUT | `/api/{resource}/{id}` | Actualizar recurso completo | `PUT /api/orders/{id}` |
| PATCH | `/api/{resource}/{id}/{action}` | Actualización parcial | `PATCH /api/orders/{id}/status` |
| DELETE | `/api/{resource}/{id}` | Eliminar recurso | `DELETE /api/orders/{id}` |

### 🔗 Endpoints Implementados

#### Orders Controller

```csharp
[Route("api/Orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]                              // GET /api/orders
    [HttpGet("{id}")]                      // GET /api/orders/{id}
    [HttpPost]                             // POST /api/orders
    [HttpPost("{id}/lines")]               // POST /api/orders/{id}/lines
    [HttpPut("{id}/lines/{lineId}")]       // PUT /api/orders/{id}/lines/{lineId}
    [HttpPatch("{id}/status")]             // PATCH /api/orders/{id}/status
    [HttpDelete("{id}/lines/{lineId}")]    // DELETE /api/orders/{id}/lines/{lineId}
}
```

#### Users Controller

```csharp
[Route("api/Users")]
public class UsersController : ControllerBase
{
    [HttpGet]                              // GET /api/users
    [HttpGet("{id}")]                      // GET /api/users/{id}
    [HttpPost]                             // POST /api/users
    [HttpPost("{id}/roles")]               // POST /api/users/{id}/roles
    [HttpDelete("{id}/roles/{roleId}")]    // DELETE /api/users/{id}/roles/{roleId}
}
```

## DTOs y Modelos de Request/Response

### 📥 Request DTOs

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/Dtos/CreateOrderRequest.cs
public sealed record CreateOrderRequest
{
    [Required]
    [StringLength(50)]
    public string PointOfSalePhoneNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DistributorPhoneNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Street { get; init; } = string.Empty;

    [StringLength(20)]
    public string? ZipCode { get; init; }

    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderLineRequest> OrderLines { get; init; } = new();
}

public sealed record CreateOrderLineRequest
{
    [Required]
    [StringLength(100)]
    public string ExternalProductId { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
```

### 📤 Response DTOs

```csharp
// Response models definidos en Application layer
public sealed record OrderResponse(
    Guid Id,
    string DistributorName,
    string PointOfSaleName,
    AddressResponse DeliveryAddress,
    string Status,
    DateTime CreatedOnUtc,
    DateTime? ConfirmedOnUtc,
    DateTime? DeliveredOnUtc,
    MoneyResponse Price,
    List<OrderLineResponse> OrderLines);

public sealed record OrderLineResponse(
    Guid Id,
    string ProductName,
    int Quantity,
    MoneyResponse SubTotal);

public sealed record AddressResponse(
    string City,
    string Street,
    string? ZipCode);

public sealed record MoneyResponse(
    decimal Amount,
    string Currency);
```

### 🔄 Mapping Extensions

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/MappingExtensions.cs
public static class OrderMappingExtensions
{
    public static CreateOrderCommand ToCommand(this CreateOrderRequest request)
    {
        return new CreateOrderCommand(
            request.PointOfSalePhoneNumber,
            request.DistributorPhoneNumber,
            request.City,
            request.Street,
            request.ZipCode ?? string.Empty,
            request.CurrencyCode,
            request.OrderLines.Select(line => new CreateOrderLineCommand(
                line.ExternalProductId,
                line.Quantity)).ToList());
    }
}
```

## Documentación con Swagger

### 📖 Configuración de OpenAPI

```csharp
// src/Conaprole.Orders.Api/Program.cs
builder.Services.AddSwaggerExamplesFromAssemblyOf<UpdateOrderStatusRequestExample>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conaprole Orders API",
        Version = "v1.1",
        Description = "API for managing dairy product orders",
        Contact = new OpenApiContact
        {
            Name = "Conaprole Development Team",
            Email = "dev@conaprole.com"
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Configuración de seguridad JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Usar nombres completos para evitar conflictos
    options.CustomSchemaIds(type => type.FullName);
    options.EnableAnnotations();
});
```

### 📋 Ejemplos para Swagger

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/Examples/CreateOrderRequestExample.cs
public class CreateOrderRequestExample : IExamplesProvider<CreateOrderRequest>
{
    public CreateOrderRequest GetExamples()
    {
        return new CreateOrderRequest
        {
            PointOfSalePhoneNumber = "123456789",
            DistributorPhoneNumber = "987654321",
            City = "Montevideo",
            Street = "18 de Julio 1234",
            ZipCode = "11000",
            CurrencyCode = "UYU",
            OrderLines = new List<CreateOrderLineRequest>
            {
                new()
                {
                    ExternalProductId = "LECHE-001",
                    Quantity = 10
                },
                new()
                {
                    ExternalProductId = "YOGURT-002", 
                    Quantity = 5
                }
            }
        };
    }
}
```

## Manejo de Errores y Respuestas

### ⚠️ Middleware de Excepciones

```csharp
// src/Conaprole.Orders.Api/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException validationException => new ErrorResponse
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Errors = validationException.Errors
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            },
            ForbiddenAccessException => new ErrorResponse
            {
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden
            },
            _ => new ErrorResponse
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError
            }
        };

        context.Response.StatusCode = response.Status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### 📋 Modelos de Error Estandarizados

```csharp
// src/Conaprole.Orders.Api/Models/ErrorResponse.cs
public sealed record ErrorResponse
{
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Detail { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
```

## Versionado de API

### 🔢 Estrategia de Versionado

```csharp
// Configuración de versionado
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new QueryStringApiVersionReader("version"));
});

// Controlador con versión
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersV1Controller : ControllerBase
{
    // Implementación v1.0
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersV2Controller : ControllerBase
{
    // Implementación v2.0 con breaking changes
}
```

## Configuración CORS

### 🌐 CORS Policy

```csharp
// src/Conaprole.Orders.Api/Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins("https://app.conaprole.com", "https://admin.conaprole.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});
```

## Paginación y Filtrado

### 📄 Parámetros de Query

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/Dtos/GetOrdersRequest.cs
public sealed record GetOrdersRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    public string? Status { get; init; }
    public Guid? DistributorId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

// Endpoint con paginación
[HttpGet]
public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequest request, CancellationToken cancellationToken)
{
    var query = new GetOrdersQuery(request.Page, request.PageSize, request.Status, request.DistributorId);
    var result = await _sender.Send(query, cancellationToken);

    if (result.IsSuccess)
    {
        // Agregar headers de paginación
        Response.Headers.Add("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Add("X-Page", result.Value.Page.ToString());
        Response.Headers.Add("X-Page-Size", result.Value.PageSize.ToString());
        
        return Ok(result.Value);
    }

    return BadRequest(result.Error);
}
```

## Content Negotiation

### 📝 Formatos Soportados

```csharp
// Configuración de formatos
builder.Services.AddControllers(options =>
{
    // Solo JSON por defecto
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
});
```

## Health Checks

### ❤️ Endpoint de Salud

```csharp
// Configuración de health checks
services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddCheck<KeycloakHealthCheck>("keycloak")
    .AddCheck("database", () => HealthCheckResult.Healthy("Database is healthy"));

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Conclusión

El diseño de API de la API Core de Conaprole implementa:

- **Convenciones REST** consistentes y predecibles
- **Documentación automática** con Swagger/OpenAPI
- **Manejo de errores** estandarizado y centralizado
- **Validación** automática de DTOs
- **Seguridad** integrada con JWT y permisos
- **Versionado** para evolución de la API
- **CORS** configurado por ambiente
- **Paginación** y filtrado en endpoints de listado
- **Health checks** para monitoreo

Esta implementación proporciona una API robusta, documentada y fácil de consumir para aplicaciones cliente.

---

*Próximo: [Patrones de Infraestructura](./infrastructure-patterns.md) - Cross-cutting concerns*

