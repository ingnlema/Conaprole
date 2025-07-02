# 🚨 Gestión de Errores y Manejo de Excepciones

## Introducción

La API Core de Conaprole implementa una **estrategia integral de gestión de errores** que combina patrones de diseño robustos con mecanismos de manejo de excepciones centralizados. El sistema adopta un enfoque **fail-fast** con **type-safety** para garantizar la consistencia, trazabilidad y experiencia de usuario óptima en el manejo de errores.

---

## 🎯 Enfoque General de Manejo de Errores

### Arquitectura de Gestión de Errores

La estrategia de manejo de errores se basa en **tres niveles fundamentales**:

1. **Nivel de Dominio**: Errores tipados y patrones Result
2. **Nivel de Aplicación**: Excepciones específicas y behaviors de validación  
3. **Nivel de Infraestructura**: Middleware centralizado y respuestas HTTP estandarizadas

### Mecanismos Principales

#### 🔧 Middleware de Excepciones Global

```csharp
// src/Conaprole.Orders.Api/Middelware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            var exceptionDetails = GetExceptionDetails(exception);
            
            var problemDetails = new ProblemDetails
            {
                Status = exceptionDetails.Status,
                Type = exceptionDetails.Type,
                Title = exceptionDetails.Title,
                Detail = exceptionDetails.Detail,
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
```

#### 🔄 Pipeline de Validación MediatR

```csharp
// src/Conaprole.Orders.Application/Abstractions/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
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

### Transformación de Errores en Respuestas HTTP

El sistema convierte automáticamente errores internos en respuestas HTTP siguiendo el estándar **RFC 7807 Problem Details**:

```csharp
private static ExceptionDetails GetExceptionDetails(Exception exception)
{
    return exception switch
    {
        ValidationException validationException => new ExceptionDetails(
            StatusCodes.Status400BadRequest,
            "ValidationFailure",
            "Validation error",
            "One or more validation errors has occurred",
            validationException.Errors),
        
        ConflictException conflictException => new ExceptionDetails(
            StatusCodes.Status409Conflict,
            "Conflict",
            "Conflict error",
            conflictException.Message,
            null),
        
        _ => new ExceptionDetails(
            StatusCodes.Status500InternalServerError,
            "ServerError",
            "Server error",
            "An unexpected error has occurred",
            null)
    };
}
```

---

## 📊 Tipos de Errores Manejados

### 1. Errores de Validación

**Contexto**: Validación de entrada de datos utilizando FluentValidation

**Mecanismo**: `ValidationBehavior` + `ValidationException`

**Implementación**:

```csharp
// src/Conaprole.Orders.Application/Exceptions/ValidationException.cs
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationError> errors)
    {
        Errors = errors;
    }

    public IEnumerable<ValidationError> Errors { get; }
}

// src/Conaprole.Orders.Application/Exceptions/ValidationError.cs
public record ValidationError(string PropertyName, string ErrorMessage);
```

**Respuesta HTTP**: `400 Bad Request` con detalles de errores de validación

### 2. Errores de Autorización y Autenticación

**Contexto**: Control de acceso y permisos de usuario

**Tipos Manejados**:

- `UnauthorizedAccessException` → `401 Unauthorized`
- `ForbiddenAccessException` → `403 Forbidden`

**Patrón de Uso**:

```csharp
// En controllers con autorización
[HasPermission(Permissions.OrdersRead)]
public async Task<IActionResult> GetOrder(Guid id)
{
    // Autorización manejada automáticamente por middleware
}
```

### 3. Excepciones de Lógica de Negocio

#### 3.1 Result Pattern para Errores de Dominio

**Implementación Base**:

```csharp
// src/Conaprole.Orders.Domain/Abstractions/Result.cs
public class Result<TValue> : Result
{
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure(Error error) => new(default, false, error);
}
```

#### 3.2 Errores de Dominio Tipados

```csharp
// src/Conaprole.Orders.Domain/Orders/OrderErrors.cs
public static class OrderErrors
{
    public static Error NotFound = new(
        "Order.NotFound",
        "The order with the specified identifier was not found");
    
    public static Error ProductNotFound = new(
        "Order.ProductNotFound",
        "The product with the specified external ID was not found.");

    public static Error DuplicateProductInOrder = new(
        "Order.DuplicateProduct",
        "The product is already part of this order.");
}
```

#### 3.3 Excepciones de Aplicación

```csharp
// Conflictos de negocio
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

// Entidades no encontradas
public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}

// Problemas de concurrencia
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
```

### 4. Fallas Técnicas

#### 4.1 Errores de Base de Datos

**Manejo**: Interceptados por middleware global y transformados en `500 Internal Server Error`

#### 4.2 Errores de Red y Servicios Externos

**Estrategia**: Logging detallado + respuestas genéricas para evitar exposición de información sensible

#### 4.3 Errores de Infraestructura

**Comportamiento**: Capturados por middleware con logging estructurado para diagnóstico

---

## 🏗️ Estrategias y Patrones Aplicados

### 1. Patrón Result<T>

**Ventajas**:

- ✅ **Type Safety**: Errores tipados sin excepciones
- ✅ **Performance**: Eliminación de overhead de excepciones
- ✅ **Explícito**: Manejo de errores visible en las firmas de métodos

**Uso en Handlers**:

```csharp
// src/Conaprole.Orders.Application/Orders/GetOrder/GetOrderQueryHandler.cs
public async Task<Result<OrderResponse>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
{
    var dbOrder = await connection.QuerySingleOrDefaultAsync<OrderResponse>(sql, new { OrderId = request.OrderId });

    if (dbOrder is null)
    {
        return Result.Failure<OrderResponse>(OrderErrors.NotFound);
    }

    return Result.Success(fullOrder);
}
```

**Uso en Controllers**:

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
{
    var query = new GetOrderQuery(id);
    var result = await _sender.Send(query, cancellationToken);

    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
}
```

### 2. Try-Catch Centralizado

**Implementación**: `ExceptionHandlingMiddleware` intercepta todas las excepciones no manejadas

**Beneficios**:

- 🔄 **Consistencia**: Respuestas uniformes
- 📊 **Observabilidad**: Logging centralizado
- 🛡️ **Seguridad**: Previene exposición de información sensible

### 3. Validation Behavior Pattern

**Característica**: Intercepta comandos antes de la ejecución para validación temprana

**Flujo**:

1. **Request** → ValidationBehavior
2. **FluentValidation** → Reglas de negocio
3. **ValidationException** → Si hay errores
4. **Handler Execution** → Solo si validación exitosa

### 4. Domain Exception Pattern

```csharp
// src/Conaprole.Orders.Domain/Exceptions/DomainException.cs
public class DomainException : Exception
{
    public DomainException() { }
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}
```

---

## 📝 Logs y Trazabilidad de Errores

### Logging Behavior

```csharp
// src/Conaprole.Orders.Application/Abstractions/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = request.GetType().Name;

        try
        {
            _logger.LogInformation("Executing command {Command}", name);
            var result = await next();
            _logger.LogInformation("Command {Command} processed successfully", name);
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Command {Command} processing failed", name);
            throw;
        }
    }
}
```

### Structured Logging en Middleware

```csharp
_logger.LogError(exception, "Exception occurred: {ExceptionType} - {ExceptionMessage}", 
    exception.GetType().Name, exception.Message);
```

**Información Capturada**:

- 🔍 **Tipo de excepción**
- 📄 **Mensaje de error**
- ⏱️ **Timestamp**
- 🎯 **Contexto de ejecución**
- 📊 **Stack trace** (para errores internos)

---

## 💡 Ejemplos de Implementación

### Ejemplo 1: Manejo de Error de Dominio

```csharp
// Handler con Result Pattern
public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
{
    var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber);
    if (pointOfSale is null)
    {
        return Result.Failure<Guid>(PointOfSaleErrors.NotFound);
    }

    // Lógica de creación...
    return Result.Success(order.Id);
}

// Controller
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    var result = await _sender.Send(command, cancellationToken);
    
    if (result.IsFailure)
        return BadRequest(result.Error);

    return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
}
```

### Ejemplo 2: Validación con FluentValidation

```csharp
// Validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty()
            .WithMessage("Point of sale phone number is required.");

        RuleFor(x => x.OrderLines)
            .NotEmpty()
            .WithMessage("At least one order line is required.");
    }
}

// Behavior intercepta y valida automáticamente
// Si hay errores → ValidationException → 400 Bad Request con detalles
```

### Ejemplo 3: Registro de Middleware

```csharp
// src/Conaprole.Orders.Api/Extensions/ApplicationBuilderExtensions.cs
public static void UseCustomExceptionHandler(this IApplicationBuilder app)
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();
}

// En Program.cs o Startup.cs
app.UseCustomExceptionHandler();
```

---

## 🔧 Componentes Clave

### Clases de Referencia

| Componente | Ubicación | Responsabilidad |
|------------|-----------|-----------------|
| `ExceptionHandlingMiddleware` | `src/Conaprole.Orders.Api/Middelware/` | Manejo centralizado de excepciones |
| `ValidationBehavior` | `src/Conaprole.Orders.Application/Abstractions/Behaviors/` | Interceptación y validación de comandos |
| `LoggingBehavior` | `src/Conaprole.Orders.Application/Abstractions/Behaviors/` | Logging estructurado de requests |
| `Result<T>` | `src/Conaprole.Orders.Domain/Abstractions/` | Patrón Result para errores tipados |
| `Error` | `src/Conaprole.Orders.Domain/Abstractions/` | Modelo base de errores de dominio |
| `*Errors` | `src/Conaprole.Orders.Domain/*/` | Colecciones de errores específicos por agregado |
| `ValidationException` | `src/Conaprole.Orders.Application/Exceptions/` | Excepción para errores de validación |

### Middlewares y Behaviors

```csharp
// Pipeline de MediatR
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(ApplicationAssembly.Assembly);
    cfg.AddBehavior<ValidationBehavior<,>>();
    cfg.AddBehavior<LoggingBehavior<,>>();
});

// Middleware de excepciones
app.UseCustomExceptionHandler();
```

---

## ✅ Beneficios de la Estrategia Implementada

### 🚀 Performance y Confiabilidad

- **Fail Fast**: Errores detectados tempranamente en el pipeline
- **Type Safety**: Eliminación de excepciones no manejadas
- **Consistent Responses**: Formato uniforme de respuestas de error

### 📊 Observabilidad y Mantenimiento

- **Structured Logging**: Logs estructurados para análisis
- **Centralized Handling**: Punto único de manejo de errores
- **Traceable Errors**: Trazabilidad completa de errores

### 👥 Experiencia del Desarrollador

- **Clear Error Types**: Tipos de error específicos y descriptivos
- **Explicit Error Handling**: Manejo explícito en controladores
- **Comprehensive Documentation**: Documentación clara de patrones

### 🔒 Seguridad

- **Information Hiding**: Errores internos no expuestos al cliente
- **Consistent Error Format**: Prevención de information leakage
- **Audit Trail**: Logging completo para auditoría

---

## 🎯 Conclusión

La implementación de gestión de errores en la API Core de Conaprole proporciona un sistema **robusto, escalable y mantenible** que:

- ✅ **Garantiza consistencia** en el manejo de errores a través de toda la aplicación
- ✅ **Proporciona type safety** mediante el patrón Result y errores tipados
- ✅ **Facilita debugging** con logging estructurado y trazabilidad completa
- ✅ **Mejora la experiencia del usuario** con respuestas de error claras y accionables
- ✅ **Mantiene la seguridad** evitando exposición de información sensible
- ✅ **Soporta escalabilidad** con patrones probados y arquitectura limpia

Esta estrategia integral asegura que la aplicación sea **resiliente, observable y fácil de mantener**, cumpliendo con los más altos estándares de calidad en sistemas empresariales.
