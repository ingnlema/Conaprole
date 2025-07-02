# ⚡ CQRS y MediatR - Implementación de Comandos y Queries

## Introducción

La API Core de Conaprole Orders implementa el patrón **CQRS (Command Query Responsibility Segregation)** utilizando **MediatR** como mediador. Esta implementación separa claramente las operaciones de lectura (Queries) de las operaciones de escritura (Commands), proporcionando una arquitectura escalable y mantenible.

## Fundamentos CQRS

### Principios Aplicados

1. **Separación de Responsabilidades**: Commands modifican estado, Queries consultan datos
2. **Modelos Independientes**: Diferentes modelos para escritura y lectura
3. **Optimización Específica**: Cada lado optimizado para su propósito
4. **Escalabilidad**: Posibilidad de escalar lectura y escritura independientemente

### Ventajas Observadas

- **Claridad de Intención**: Cada operación tiene un propósito específico
- **Rendimiento Optimizado**: Queries optimizadas para lectura
- **Mantenibilidad**: Casos de uso bien definidos
- **Testabilidad**: Cada handler se puede probar independientemente

## Implementación con MediatR

### Configuración Base

```csharp
// src/Conaprole.Orders.Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(configuration =>
    {
        configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        
        // Pipeline behaviors
        configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    return services;
}
```

### Abstracciones Base

```csharp
// src/Conaprole.Orders.Application/Abstractions/Messaging/ICommand.cs
public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

// src/Conaprole.Orders.Application/Abstractions/Messaging/IQuery.cs
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

// src/Conaprole.Orders.Application/Abstractions/Messaging/ICommandHandler.cs
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}

// src/Conaprole.Orders.Application/Abstractions/Messaging/IQueryHandler.cs
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
```

## Commands (Operaciones de Escritura)

### Estructura de Commands

#### CreateOrder Command

```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommand.cs
public sealed record CreateOrderCommand(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<CreateOrderLineCommand> OrderLines) : ICommand<Guid>;

public sealed record CreateOrderLineCommand(
    string ExternalProductId,
    int Quantity);
```

#### CreateOrder Command Handler

```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommandHandler.cs
internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IDistributorRepository _distributorRepository;

    public CreateOrderCommandHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IPointOfSaleRepository pointOfSaleRepository,
        IDistributorRepository distributorRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _pointOfSaleRepository = pointOfSaleRepository;
        _distributorRepository = distributorRepository;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar entidades relacionadas
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(
            request.PointOfSalePhoneNumber, cancellationToken);
        if (pointOfSale is null)
            return Result.Failure<Guid>(new Error("Order.InvalidPointOfSale", "Point of Sale not found."));

        var distributor = await _distributorRepository.GetByPhoneNumberAsync(
            request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<Guid>(new Error("Order.InvalidDistributor", "Distributor not found."));

        // 2. Crear objetos de dominio
        var address = new Address(request.City, request.Street, request.ZipCode);
        var currency = Currency.FromCode(request.CurrencyCode);
        
        var order = new Order(
            Guid.NewGuid(),
            pointOfSale.Id,
            pointOfSale,
            distributor.Id,
            distributor,
            address,
            Status.Created,
            _dateTimeProvider.UtcNow,
            confirmedOnUtc: null,
            rejectedOnUtc: null,
            deliveryOnUtc: null,
            canceledOnUtc: null,
            deliveredOnUtc: null,
            new Money(0, currency));

        // 3. Agregar líneas de pedido
        foreach (var line in request.OrderLines)
        {
            var product = await _productRepository.GetByExternalIdAsync(
                new ExternalProductId(line.ExternalProductId), cancellationToken);
            if (product is null)
                return Result.Failure<Guid>(ProductErrors.NotFound);

            var quantity = new Quantity(line.Quantity);
            var subtotal = product.UnitPrice * quantity;
            var orderLine = new OrderLine(
                Guid.NewGuid(),
                quantity,
                subtotal,
                product,
                new OrderId(order.Id),
                _dateTimeProvider.UtcNow);

            order.AddOrderLine(orderLine);
        }

        // 4. Persistir y confirmar transacción
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
```

#### Command Validation

```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommandValidator.cs
internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty()
            .WithMessage("PointOfSale phone number is required");

        RuleFor(x => x.DistributorPhoneNumber)
            .NotEmpty()
            .WithMessage("Distributor phone number is required");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency code must be 3 characters");

        RuleFor(x => x.OrderLines)
            .NotEmpty()
            .WithMessage("Order must have at least one line");

        RuleForEach(x => x.OrderLines)
            .SetValidator(new CreateOrderLineCommandValidator());
    }
}

internal sealed class CreateOrderLineCommandValidator : AbstractValidator<CreateOrderLineCommand>
{
    public CreateOrderLineCommandValidator()
    {
        RuleFor(x => x.ExternalProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}
```

### Otros Commands Implementados

#### UpdateOrderStatus Command

```csharp
// src/Conaprole.Orders.Application/Orders/UpdateOrderStatus/UpdateOrderStatusCommand.cs
public sealed record UpdateOrderStatusCommand(Guid OrderId, int Status) : ICommand;

internal sealed class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var status = Status.FromValue(request.Status);
        order.UpdateStatus(status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

#### AddOrderLine Command

```csharp
// src/Conaprole.Orders.Application/Orders/AddOrderLine/AddOrderLineCommand.cs
public sealed record AddOrderLineCommand(
    Guid OrderId,
    string ExternalProductId,
    int Quantity) : ICommand;

internal sealed class AddOrderLineCommandHandler : ICommandHandler<AddOrderLineCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public async Task<Result> Handle(AddOrderLineCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var product = await _productRepository.GetByExternalIdAsync(
            new ExternalProductId(request.ExternalProductId), cancellationToken);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound);

        var quantity = new Quantity(request.Quantity);
        var subtotal = product.UnitPrice * quantity;
        var orderLine = new OrderLine(
            Guid.NewGuid(),
            quantity,
            subtotal,
            product,
            new OrderId(order.Id),
            _dateTimeProvider.UtcNow);

        order.AddOrderLine(orderLine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

## Queries (Operaciones de Lectura)

### Estructura de Queries

#### GetOrder Query

```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/GetOrderQuery.cs
public sealed record GetOrderQuery(Guid Id) : IQuery<OrderResponse>;

// src/Conaprole.Orders.Application/Orders/GetOrder/OrderResponse.cs
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
    string ZipCode);

public sealed record MoneyResponse(
    decimal Amount,
    string Currency);
```

#### GetOrder Query Handler

```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/GetOrderQueryHandler.cs
internal sealed class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderResponse>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetOrderQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                o.id,
                d.name as distributor_name,
                pos.name as point_of_sale_name,
                o.delivery_address_city,
                o.delivery_address_street,
                o.delivery_address_zip_code,
                o.status,
                o.created_on_utc,
                o.confirmed_on_utc,
                o.delivered_on_utc,
                o.price_amount,
                o.price_currency,
                ol.id as line_id,
                p.name as product_name,
                ol.quantity,
                ol.sub_total_amount,
                ol.sub_total_currency
            FROM orders o
            INNER JOIN distributors d ON o.distributor_id = d.id
            INNER JOIN points_of_sale pos ON o.point_of_sale_id = pos.id
            LEFT JOIN order_lines ol ON o.id = ol.order_id
            LEFT JOIN products p ON ol.product_id = p.id
            WHERE o.id = @OrderId
            """;

        var orderDictionary = new Dictionary<Guid, OrderResponse>();

        var orders = await connection.QueryAsync<OrderResponse, OrderLineResponse, OrderResponse>(
            sql,
            (order, orderLine) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order with { OrderLines = new List<OrderLineResponse>() };
                    orderDictionary.Add(order.Id, existingOrder);
                }

                if (orderLine is not null)
                {
                    existingOrder.OrderLines.Add(orderLine);
                }

                return existingOrder;
            },
            new { OrderId = request.Id },
            splitOn: "line_id");

        var orderResponse = orderDictionary.Values.FirstOrDefault();
        return orderResponse is not null 
            ? Result.Success(orderResponse) 
            : Result.Failure<OrderResponse>(OrderErrors.NotFound);
    }
}
```

#### GetOrders Query (con Paginación)

```csharp
// src/Conaprole.Orders.Application/Orders/GetOrders/GetOrdersQuery.cs
public sealed record GetOrdersQuery(
    int Page,
    int PageSize,
    string? Status = null,
    Guid? DistributorId = null) : IQuery<PagedResult<OrderSummaryResponse>>;

public sealed record OrderSummaryResponse(
    Guid Id,
    string DistributorName,
    string PointOfSaleName,
    string Status,
    DateTime CreatedOnUtc,
    decimal TotalAmount,
    string Currency);

public sealed record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage,
    bool HasPreviousPage);
```

### Query Optimization Patterns

#### Projection con Dapper

```csharp
// Uso de Dapper para queries optimizadas
internal sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderSummaryResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public async Task<Result<PagedResult<OrderSummaryResponse>>> Handle(
        GetOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var whereClause = BuildWhereClause(request);
        var parameters = BuildParameters(request);

        // Query para contar total de registros
        var countSql = $"SELECT COUNT(*) FROM orders o INNER JOIN distributors d ON o.distributor_id = d.id {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // Query para obtener datos paginados
        var dataSql = $"""
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
            {whereClause}
            ORDER BY o.created_on_utc DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var orders = await connection.QueryAsync<OrderSummaryResponse>(dataSql, parameters);

        var result = new PagedResult<OrderSummaryResponse>(
            orders.ToList(),
            request.Page,
            request.PageSize,
            totalCount,
            HasNextPage: (request.Page * request.PageSize) < totalCount,
            HasPreviousPage: request.Page > 1);

        return Result.Success(result);
    }
}
```

## Pipeline Behaviors

### Validation Behavior

```csharp
// src/Conaprole.Orders.Application/Abstractions/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationErrors = _validators
            .Select(validator => validator.Validate(context))
            .Where(validationResult => validationResult.Errors.Any())
            .SelectMany(validationResult => validationResult.Errors)
            .Select(validationFailure => new ValidationError(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage))
            .ToList();

        if (validationErrors.Any())
        {
            throw new ValidationException(validationErrors);
        }

        return await next();
    }
}
```

### Logging Behavior

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

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;

        try
        {
            _logger.LogInformation("Executing request {RequestName}", name);

            var result = await next();

            _logger.LogInformation("Request {RequestName} processed successfully", name);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Request {RequestName} processing failed", name);

            throw;
        }
    }
}
```

## Integración con Controllers

### Uso en Controllers

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
[ApiController]
[Route("api/Orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.PointOfSalePhoneNumber,
            request.DistributorPhoneNumber,
            request.City,
            request.Street,
            request.ZipCode,
            request.CurrencyCode,
            request.OrderLines.Select(line => new CreateOrderLineCommand(
                line.ExternalProductId,
                line.Quantity)).ToList());

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id, 
        UpdateOrderStatusRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(id, request.Status);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
```

## Patrones de Resultado

### Result Pattern

```csharp
// src/Conaprole.Orders.Domain/Abstractions/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        Value = value;
    }
}
```

## Métricas de Implementación

### Commands Implementados

- **CreateOrder**: Creación de pedidos con validación completa
- **UpdateOrderStatus**: Actualización de estado de pedidos
- **AddOrderLine**: Adición de líneas a pedidos existentes
- **RemoveOrderLine**: Eliminación de líneas de pedido
- **UpdateOrderLineQuantity**: Actualización de cantidades
- **BulkCreateOrders**: Creación masiva de pedidos

### Queries Implementados

- **GetOrder**: Obtención de pedido por ID con datos relacionados
- **GetOrders**: Listado paginado con filtros opcionales
- **GetOrdersByDistributor**: Pedidos por distribuidor
- **GetOrdersByStatus**: Pedidos por estado
- **GetOrdersByDateRange**: Pedidos por rango de fechas

### Behaviors Configurados

- **ValidationBehavior**: Validación automática de commands
- **LoggingBehavior**: Logging estructurado de operaciones
- **TransactionBehavior**: Manejo de transacciones (futuro)
- **CachingBehavior**: Cacheo de queries (futuro)

## Conclusión

La implementación de CQRS con MediatR en la API Core de Conaprole proporciona:

- **Separación clara** entre operaciones de lectura y escritura
- **Optimización específica** para cada tipo de operación
- **Pipeline behaviors** para cross-cutting concerns
- **Validación automática** de comandos
- **Logging centralizado** de operaciones
- **Testabilidad** alta de cada handler
- **Escalabilidad** y mantenibilidad del código

Esta arquitectura facilita el crecimiento del sistema y la adición de nuevas funcionalidades sin afectar el código existente.

---

*Próximo: [Arquitectura de Seguridad](./security-architecture.md) - Autenticación, autorización y permisos*
