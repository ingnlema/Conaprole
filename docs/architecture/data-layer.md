# 🗄️ Capa de Datos - Persistencia y Repositorios

## Introducción

La capa de datos de la API Core de Conaprole Orders implementa el **Repository Pattern** y utiliza **Entity Framework Core** con **PostgreSQL** como base de datos principal. Esta arquitectura proporciona una abstracción limpia para el acceso a datos, siguiendo los principios de Clean Architecture y manteniendo la independencia del dominio respecto a los detalles de persistencia.

## Tecnologías y Patrones

### 🛠️ Stack Tecnológico
- **Entity Framework Core 8.0**: ORM principal
- **PostgreSQL**: Base de datos relacional
- **Npgsql**: Provider para PostgreSQL
- **Dapper**: Queries optimizadas para consultas complejas
- **Snake Case Naming**: Convención de nomenclatura de BD

### 📐 Patrones Implementados
- **Repository Pattern**: Abstracción de acceso a datos
- **Unit of Work**: Manejo de transacciones
- **Specification Pattern**: Consultas complejas reutilizables
- **CQRS**: Separación de comandos y queries

## DbContext y Configuración

### 🏛️ ApplicationDbContext
```csharp
// src/Conaprole.Orders.Infrastructure/ApplicationDbContext.cs
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLine> OrderLines { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Distributor> Distributors { get; set; } = null!;
    public DbSet<PointOfSale> PointsOfSale { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas las configuraciones del assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Manejar entidades Role que pueden estar detached
            HandleRoleEntities();

            var result = await base.SaveChangesAsync(cancellationToken);

            // Publicar domain events después del commit
            await PublishDomainEventsAsync();

            return result;
        }
        catch (DbUpdateException ex)
        {
            throw new DataAccessException("Error saving changes to database", ex);
        }
    }

    private void HandleRoleEntities()
    {
        foreach (var entry in ChangeTracker.Entries<Role>())
        {
            if (entry.State == EntityState.Added && entry.Entity.Id > 0)
            {
                // Si es un rol estático con ID, marcarlo como unchanged
                entry.State = EntityState.Unchanged;
            }
        }
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.GetDomainEvents().Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.GetDomainEvents())
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent);
        }
    }
}
```

### ⚙️ Configuración de Servicios
```csharp
// src/Conaprole.Orders.Infrastructure/DependencyInjection.cs
private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Database") 
        ?? throw new ArgumentNullException(nameof(configuration));

    // Configuración de DbContext
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
    });

    // Registro de repositorios
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IRoleRepository, RoleRepository>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<IProductRepository, ProductRepository>();
    services.AddScoped<IDistributorRepository, DistributorRepository>();
    services.AddScoped<IPointOfSaleRepository, PointOfSaleRepository>();
    services.AddScoped<IPointOfSaleDistributorRepository, PointOfSaleDistributorRepository>();

    // Unit of Work
    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

    // SQL Connection Factory para Dapper
    services.AddSingleton<ISqlConnectionFactory>(_ => 
        new SqlConnectionFactory(connectionString));

    // Type Handlers para tipos custom
    SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
}
```

## Entity Configurations

### 🏗️ Configuraciones de Entidades

#### Order Configuration
```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/OrderConfiguration.cs
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        // Configuración de Address como Owned Type
        builder.OwnsOne(order => order.DeliveryAddress, addressBuilder =>
        {
            addressBuilder.Property(address => address.City)
                .HasMaxLength(100)
                .HasColumnName("delivery_address_city");

            addressBuilder.Property(address => address.Street)
                .HasMaxLength(200)
                .HasColumnName("delivery_address_street");

            addressBuilder.Property(address => address.ZipCode)
                .HasMaxLength(20)
                .HasColumnName("delivery_address_zip_code");
        });

        // Configuración de Money como Owned Type
        builder.OwnsOne(order => order.Price, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("price_amount");

            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("price_currency");
        });

        // Configuración de Status como Value Conversion
        builder.Property(order => order.Status)
            .HasConversion(
                status => status.Value,
                value => Status.FromValue(value));

        // Relaciones
        builder.HasOne<Distributor>()
            .WithMany()
            .HasForeignKey(order => order.DistributorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PointOfSale>()
            .WithMany()
            .HasForeignKey(order => order.PointOfSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.OrderLines)
            .WithOne()
            .HasForeignKey(orderLine => orderLine.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(order => order.CreatedOnUtc);
        builder.HasIndex(order => order.Status);
        builder.HasIndex(order => order.DistributorId);
        builder.HasIndex(order => order.PointOfSaleId);
    }
}
```

#### OrderLine Configuration
```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/OrderLineConfiguration.cs
internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(orderLine => orderLine.Id);

        builder.Property(orderLine => orderLine.Id)
            .ValueGeneratedNever();

        // Configuración de Quantity como Value Object
        builder.OwnsOne(orderLine => orderLine.Quantity, quantityBuilder =>
        {
            quantityBuilder.Property(quantity => quantity.Value)
                .HasColumnName("quantity");
        });

        // Configuración de SubTotal como Owned Type
        builder.OwnsOne(orderLine => orderLine.SubTotal, subTotalBuilder =>
        {
            subTotalBuilder.Property(money => money.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("sub_total_amount");

            subTotalBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("sub_total_currency");
        });

        // Configuración de OrderId como Value Object
        builder.OwnsOne(orderLine => orderLine.OrderId, orderIdBuilder =>
        {
            orderIdBuilder.Property(orderId => orderId.Value)
                .HasColumnName("order_id");
        });

        // Relación con Product
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(orderLine => orderLine.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(orderLine => orderLine.OrderId);
        builder.HasIndex(orderLine => orderLine.ProductId);
    }
}
```

#### User Configuration
```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/UserConfiguration.cs
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        // Configuración de Value Objects
        builder.OwnsOne(user => user.FirstName, firstNameBuilder =>
        {
            firstNameBuilder.Property(firstName => firstName.Value)
                .HasMaxLength(200)
                .HasColumnName("first_name");
        });

        builder.OwnsOne(user => user.LastName, lastNameBuilder =>
        {
            lastNameBuilder.Property(lastName => lastName.Value)
                .HasMaxLength(200)
                .HasColumnName("last_name");
        });

        builder.OwnsOne(user => user.Email, emailBuilder =>
        {
            emailBuilder.Property(email => email.Value)
                .HasMaxLength(400)
                .HasColumnName("email");

            emailBuilder.HasIndex(email => email.Value)
                .IsUnique();
        });

        builder.Property(user => user.IdentityId)
            .HasMaxLength(400);

        builder.HasIndex(user => user.IdentityId)
            .IsUnique();

        // Relación muchos a muchos con Roles
        builder.HasMany(user => user.Roles)
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable("user_roles");
                joinBuilder.Property("UsersId").HasColumnName("user_id");
                joinBuilder.Property("RolesId").HasColumnName("role_id");
            });
    }
}
```

#### Product Configuration
```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/ProductConfiguration.cs
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        // Configuración de Name como Value Object
        builder.OwnsOne(product => product.Name, nameBuilder =>
        {
            nameBuilder.Property(name => name.Value)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        // Configuración de ExternalProductId
        builder.OwnsOne(product => product.ExternalId, externalIdBuilder =>
        {
            externalIdBuilder.Property(externalId => externalId.Value)
                .HasMaxLength(100)
                .HasColumnName("external_id");

            externalIdBuilder.HasIndex(externalId => externalId.Value)
                .IsUnique();
        });

        // Configuración de UnitPrice como Owned Type
        builder.OwnsOne(product => product.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(money => money.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("unit_price_amount");

            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("unit_price_currency");
        });

        // Configuración de Category como enum
        builder.Property(product => product.Category)
            .HasConversion<string>();
    }
}
```

## Repository Pattern Implementation

### 🏛️ Base Repository
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/Repository.cs
internal abstract class Repository<T>
    where T : Entity
{
    protected readonly ApplicationDbContext DbContext;

    protected Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual void Add(T entity)
    {
        DbContext.Set<T>().Add(entity);
    }

    public virtual void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }

    public virtual async Task<T?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>()
            .AnyAsync(e => e.Id == id, cancellationToken);
    }
}
```

### 📦 Implementaciones Específicas

#### Order Repository
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/OrderRepository.cs
internal sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<Order?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Order>()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Include(o => o.Distributor)
            .Include(o => o.PointOfSale)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetByDistributorIdAsync(
        Guid distributorId, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Order>()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Where(o => o.DistributorId == distributorId)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByStatusAsync(
        Status status, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Order>()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Order>()
            .Include(o => o.OrderLines)
                .ThenInclude(ol => ol.Product)
            .Where(o => o.CreatedOnUtc >= startDate && o.CreatedOnUtc <= endDate)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }
}
```

#### User Repository
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/UserRepository.cs
internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(
        Email email, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdentityIdAsync(
        string identityId, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(
        Email email, 
        CancellationToken cancellationToken = default)
    {
        return !await DbContext.Set<User>()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public override async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<User>()
            .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
            .OrderBy(u => u.FirstName.Value)
            .ThenBy(u => u.LastName.Value)
            .ToListAsync(cancellationToken);
    }
}
```

#### Product Repository
```csharp
// src/Conaprole.Orders.Infrastructure/Repositories/ProductRepository.cs
internal sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Product?> GetByExternalIdAsync(
        ExternalProductId externalId, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Product>()
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, cancellationToken);
    }

    public async Task<List<Product>> GetByCategoryAsync(
        Category category, 
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Product>()
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsExternalIdUniqueAsync(
        ExternalProductId externalId, 
        CancellationToken cancellationToken = default)
    {
        return !await DbContext.Set<Product>()
            .AnyAsync(p => p.ExternalId == externalId, cancellationToken);
    }
}
```

## Unit of Work Pattern

### 🔄 IUnitOfWork Implementation
```csharp
// src/Conaprole.Orders.Domain/Abstractions/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// ApplicationDbContext ya implementa IUnitOfWork
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    // Implementación vista anteriormente
}
```

### 🎯 Uso en Command Handlers
```csharp
// Ejemplo de uso en CreateOrderCommandHandler
public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
{
    // 1. Crear la entidad de dominio
    var order = CreateOrderFromCommand(request);

    // 2. Agregar al repositorio
    _orderRepository.Add(order);

    // 3. Confirmar cambios (Unit of Work)
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(order.Id);
}
```

## Query Optimization con Dapper

### ⚡ SQL Connection Factory
```csharp
// src/Conaprole.Orders.Infrastructure/Data/SqlConnectionFactory.cs
internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
```

### 📊 Queries Optimizadas
```csharp
// Ejemplo en GetOrdersQueryHandler
public async Task<Result<PagedResult<OrderSummaryResponse>>> Handle(
    GetOrdersQuery request, 
    CancellationToken cancellationToken)
{
    using var connection = _sqlConnectionFactory.CreateConnection();

    var sql = """
        SELECT 
            o.id,
            d.name as distributor_name,
            pos.name as point_of_sale_name,
            o.status,
            o.created_on_utc,
            o.price_amount as total_amount,
            o.price_currency as currency
        FROM orders o
        INNER JOIN distributors d ON o.distributor_id = d.id
        INNER JOIN points_of_sale pos ON o.point_of_sale_id = pos.id
        WHERE (@Status IS NULL OR o.status = @Status)
          AND (@DistributorId IS NULL OR o.distributor_id = @DistributorId)
        ORDER BY o.created_on_utc DESC
        LIMIT @PageSize OFFSET @Offset
        """;

    var orders = await connection.QueryAsync<OrderSummaryResponse>(sql, new
    {
        Status = request.Status,
        DistributorId = request.DistributorId,
        PageSize = request.PageSize,
        Offset = (request.Page - 1) * request.PageSize
    });

    return Result.Success(CreatePagedResult(orders, request));
}
```

## Migrations y Schema

### 🔧 Migration Strategy
```csharp
// Configuración en Program.cs
var applyMigrations = builder.Configuration.GetValue<bool>("APPLY_MIGRATIONS");
if (applyMigrations || app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

// Extension method
public static void ApplyMigrations(this IApplicationBuilder app)
{
    using var scope = app.ApplicationServices.CreateScope();
    using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    dbContext.Database.Migrate();
}
```

### 📋 Schema Example (PostgreSQL)
```sql
-- Tabla orders generada por EF Core
CREATE TABLE orders (
    id uuid NOT NULL,
    distributor_id uuid NOT NULL,
    point_of_sale_id uuid NOT NULL,
    delivery_address_city character varying(100) NOT NULL,
    delivery_address_street character varying(200) NOT NULL,
    delivery_address_zip_code character varying(20),
    status integer NOT NULL,
    created_on_utc timestamp with time zone NOT NULL,
    confirmed_on_utc timestamp with time zone,
    delivered_on_utc timestamp with time zone,
    price_amount numeric(18,2) NOT NULL,
    price_currency character varying(3) NOT NULL,
    CONSTRAINT pk_orders PRIMARY KEY (id)
);

-- Índices
CREATE INDEX ix_orders_created_on_utc ON orders (created_on_utc);
CREATE INDEX ix_orders_distributor_id ON orders (distributor_id);
CREATE INDEX ix_orders_point_of_sale_id ON orders (point_of_sale_id);
CREATE INDEX ix_orders_status ON orders (status);
```

## Type Handlers y Conversions

### 🔄 Custom Type Handlers
```csharp
// src/Conaprole.Orders.Infrastructure/Data/DateOnlyTypeHandler.cs
internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        return DateOnly.FromDateTime((DateTime)value);
    }
}
```

### ⚙️ Value Conversions
```csharp
// Ejemplo de conversión de Currency en configuración
builder.Property(order => order.Price)
    .OwnsOne(price => price.Currency)
    .Property(currency => currency.Code)
    .HasConversion(
        currency => currency.Code,
        code => Currency.FromCode(code))
    .HasMaxLength(3);
```

## Performance y Optimización

### 🚀 Estrategias Implementadas

1. **Eager Loading Selectivo**: Solo cargar relaciones necesarias
2. **Projection con Dapper**: Queries read-only optimizadas
3. **Índices Estratégicos**: En columnas frecuentemente consultadas
4. **Connection Pooling**: Configuración automática con Npgsql
5. **Query Splitting**: Para evitar consultas cartesianas

### 📈 Métricas de Rendimiento
```csharp
// Ejemplo de query splitting para evitar cartesian products
return await DbContext.Set<Order>()
    .AsSplitQuery() // Split para evitar cartesian product
    .Include(o => o.OrderLines)
        .ThenInclude(ol => ol.Product)
    .Include(o => o.Distributor)
    .Include(o => o.PointOfSale)
    .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
```

## Conclusión

La capa de datos de la API Core de Conaprole implementa una arquitectura robusta que combina:

- **Repository Pattern** para abstracción limpia
- **Entity Framework Core** para ORM completo
- **Dapper** para queries optimizadas
- **Unit of Work** para manejo de transacciones
- **Value Objects** correctamente mapeados
- **Domain Events** publicados automáticamente
- **Optimizaciones de rendimiento** específicas

Esta implementación asegura la separación entre el dominio y los detalles de persistencia, manteniendo alta performance y facilidad de mantenimiento.

---

*Próximo: [Inyección de Dependencias](./dependency-injection.md) - Configuración del contenedor IoC*