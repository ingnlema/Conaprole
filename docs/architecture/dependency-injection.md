# 🔌 Inyección de Dependencias - Configuración del Contenedor IoC

## Introducción

La API Core de Conaprole Orders utiliza el **contenedor de inyección de dependencias integrado de .NET** para gestionar las dependencias de forma limpia y organizada. La configuración sigue el patrón de **Extension Methods** para mantener la separación de responsabilidades por capas.

## Estructura de Configuración

### 🏗️ Organización por Capas

```csharp
// src/Conaprole.Orders.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuración por capas
builder.Services.AddApplication();                    // Capa de Aplicación
builder.Services.AddInfrastructure(configuration);    // Capa de Infraestructura
// La capa de dominio no requiere registro (POCO entities)
```

### 📱 Application Layer Configuration

```csharp
// src/Conaprole.Orders.Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR para CQRS
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            
            // Pipeline behaviors
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
```

### 🏗️ Infrastructure Layer Configuration

```csharp
// src/Conaprole.Orders.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Servicios base
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        
        // Configuración modular
        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") 
            ?? throw new ArgumentNullException(nameof(configuration));

        // Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        // Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IDistributorRepository, DistributorRepository>();
        services.AddScoped<IPointOfSaleRepository, PointOfSaleRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Dapper
        services.AddSingleton<ISqlConnectionFactory>(_ => 
            new SqlConnectionFactory(connectionString));
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        // Keycloak services
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();
        services.AddHttpClient<IJwtService, JwtService>();
        
        services.AddScoped<IUserContext, UserContext>();
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddScoped<AuthorizationService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
    }
}
```

## Patrones de Registro

### 🔄 Lifetimes Utilizados

- **Transient**: Servicios sin estado que se crean cada vez
- **Scoped**: Servicios por request HTTP
- **Singleton**: Servicios compartidos durante toda la aplicación

```csharp
// Ejemplos de diferentes lifetimes
services.AddTransient<IDateTimeProvider, DateTimeProvider>();          // Stateless utility
services.AddScoped<IUserRepository, UserRepository>();                // Por request
services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>(); // Shared resource
```

### 🎯 Configuración de Options Pattern

```csharp
// Configuración de opciones tipadas
services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));
services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

// Con validación
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## Configuración de Servicios Externos

### 🌐 HttpClient Configuration

```csharp
// Keycloak Admin API
services.AddHttpClient<IAuthenticationService, AuthenticationService>((serviceProvider, httpClient) =>
{
    var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
    httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
})
.AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>();

// JWT Token Service
services.AddHttpClient<IJwtService, JwtService>((serviceProvider, httpClient) =>
{
    var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
    httpClient.BaseAddress = new Uri(keycloakOptions.TokenUrl);
});
```

### 📊 Logging Configuration

```csharp
// Program.cs - Serilog configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog();
```

## Configuración de Testing

### 🧪 Test Container Configuration

```csharp
// Test-specific DI configuration
public class TestingWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real database with test database
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Mock external services
            services.AddScoped<IAuthenticationService, MockAuthenticationService>();
        });
    }
}
```

## Validación de Configuración

### ✅ Health Checks

```csharp
services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddCheck<KeycloakHealthCheck>("keycloak")
    .AddCheck<DatabaseHealthCheck>("database");
```

---

*Próximo: [Estrategia de Testing](./testing-strategy.md) - Arquitectura de pruebas*
