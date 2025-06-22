# 🏗️ Patrones de Infraestructura - Cross-cutting Concerns

## Introducción

La API Core de Conaprole Orders implementa diversos **patrones de infraestructura** y **cross-cutting concerns** que proporcionan funcionalidades transversales a toda la aplicación. Estos patrones aseguran aspectos como logging, manejo de errores, validación, configuración y monitoreo de manera consistente.

## Logging con Serilog

### 📝 Configuración Estructurada

```csharp
// src/Conaprole.Orders.Api/Program.cs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Conaprole.Orders.Api")
    .CreateLogger();

builder.Host.UseSerilog();
```

### 🔍 Logging Behavior en MediatR

```csharp
// src/Conaprole.Orders.Application/Abstractions/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestName"] = requestName
        }))
        {
            _logger.LogInformation("Processing request {RequestName} with ID {RequestId}", requestName, requestId);

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var result = await next();
                stopwatch.Stop();

                _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Request {RequestName} with ID {RequestId} failed", requestName, requestId);
                throw;
            }
        }
    }
}
```

### 📊 Logging Estructurado

```csharp
// Ejemplos de logging estructurado en diferentes capas
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for PointOfSale {PointOfSalePhone} and Distributor {DistributorPhone}", 
            request.PointOfSalePhoneNumber, request.DistributorPhoneNumber);

        try
        {
            var order = await CreateOrderAsync(request);
            
            _logger.LogInformation("Order {OrderId} created successfully with {LineCount} lines", 
                order.Id, order.OrderLines.Count);

            return Result.Success(order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for PointOfSale {PointOfSalePhone}", 
                request.PointOfSalePhoneNumber);
            throw;
        }
    }
}
```

## Exception Handling Middleware

### ⚠️ Manejo Centralizado de Excepciones

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
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred",
                    Extensions = { ["errors"] = validationEx.Errors }
                }
            ),
            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Domain Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = domainEx.Message
                }
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Authentication is required"
                }
            ),
            ForbiddenAccessException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Title = "Forbidden",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = forbiddenEx.Message
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred"
                }
            )
        };

        _logger.LogError(exception, "Exception occurred: {ExceptionType} - {ExceptionMessage}", 
            exception.GetType().Name, exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
```

### 🔧 Extension Method para Registro

```csharp
// src/Conaprole.Orders.Api/Extensions/ApplicationBuilderExtensions.cs
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.Migrate();
    }
}
```

## Configuration Management

### ⚙️ Patrón Options

```csharp
// src/Conaprole.Orders.Infrastructure/Authentication/Models/AuthenticationOptions.cs
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required] 
    public string MetadataUrl { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}

// Configuración y validación
services.AddOptions<AuthenticationOptions>()
    .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 📋 Configuración por Ambiente

```json
// appsettings.Development.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Information"
      }
    }
  },
  "Authentication": {
    "RequireHttpsMetadata": false
  },
  "APPLY_MIGRATIONS": true
}

// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Authentication": {
    "RequireHttpsMetadata": true
  },
  "APPLY_MIGRATIONS": false
}
```

## Health Checks

### ❤️ Verificaciones de Salud

```csharp
// src/Conaprole.Orders.Infrastructure/HealthChecks/DatabaseHealthCheck.cs
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

// Configuración
services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<KeycloakHealthCheck>("keycloak");

// Endpoint con detalles
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});
```

## Caching Strategies

### 🗃️ Response Caching

```csharp
// Configuración de caching
services.AddResponseCaching();
services.AddMemoryCache();

// Uso en controllers
[HttpGet("{id}")]
[ResponseCache(Duration = 300, VaryByHeader = "Authorization")] // 5 minutos
public async Task<IActionResult> GetProduct(Guid id)
{
    // Implementation
}
```

### 🔄 Distributed Caching (Futuro)

```csharp
// Configuración para Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

// Wrapper para caching
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
```

## Background Services

### ⏰ Hosted Services

```csharp
// src/Conaprole.Orders.Infrastructure/BackgroundServices/OrderProcessingService.cs
public class OrderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(IServiceProvider serviceProvider, ILogger<OrderProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending orders");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingOrders = await orderRepository.GetByStatusAsync(Status.Created, cancellationToken);

        foreach (var order in pendingOrders)
        {
            // Process order logic
            _logger.LogInformation("Processing order {OrderId}", order.Id);
        }
    }
}

// Registro
services.AddHostedService<OrderProcessingService>();
```

## API Versioning

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

services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
```

## Request/Response Compression

### 📦 Compresión HTTP

```csharp
// Configuración de compresión
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// Uso
app.UseResponseCompression();
```

## Security Headers

### 🛡️ Headers de Seguridad

```csharp
// Middleware personalizado para headers de seguridad
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}

// Registro
app.UseMiddleware<SecurityHeadersMiddleware>();
```

## Performance Monitoring

### 📊 Application Insights (Futuro)

```csharp
// Configuración de telemetría
services.AddApplicationInsightsTelemetry();

// Custom telemetry
public class TelemetryService
{
    private readonly TelemetryClient _telemetryClient;

    public TelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackOrderCreated(Guid orderId, decimal amount)
    {
        _telemetryClient.TrackEvent("OrderCreated", new Dictionary<string, string>
        {
            ["OrderId"] = orderId.ToString(),
            ["Amount"] = amount.ToString()
        });
    }
}
```

## Environment-Specific Configuration

### 🌍 Configuración por Ambiente

```csharp
// Program.cs - Configuración condicional
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDeveloperServices();
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddProductionServices();
}

// Extension methods para servicios específicos
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeveloperServices(this IServiceCollection services)
    {
        services.AddDeveloperExceptionPage();
        // Otros servicios de desarrollo
        return services;
    }

    public static IServiceCollection AddProductionServices(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
        
        return services;
    }
}
```

## Rate Limiting

### 🚦 Limitación de Tasa (Futuro)

```csharp
// Configuración de rate limiting
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

## Conclusión

Los patrones de infraestructura implementados en la API Core de Conaprole proporcionan:

- **Logging estructurado** con Serilog para observabilidad completa
- **Manejo centralizado de excepciones** con respuestas consistentes
- **Configuración tipada** con validación automática
- **Health checks** para monitoreo de dependencias
- **Security headers** para protección contra vulnerabilidades
- **Compresión HTTP** para optimización de performance
- **Background services** para tareas asíncronas
- **Configuración por ambiente** para flexibilidad de despliegue

Esta infraestructura robusta asegura que la aplicación sea:
- **Observable**: Logs estructurados y health checks
- **Resiliente**: Manejo de errores y reintentos
- **Segura**: Headers de seguridad y validaciones
- **Performante**: Caching y compresión
- **Mantenible**: Configuración clara y extensible

---

*Fin de la documentación arquitectónica. Ver [Resumen](./resumen.md) para una visión general completa.*