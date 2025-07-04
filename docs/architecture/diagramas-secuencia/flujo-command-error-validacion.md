# ⚠️ Flujo de Command con Error de Validación

## 📋 Descripción

Este diagrama representa el flujo alternativo cuando un comando falla debido a errores de validación utilizando FluentValidation. Muestra cómo el sistema maneja y responde a datos de entrada inválidos de manera consistente.

## 🏗️ Arquitectura de Manejo de Errores

- **ValidationBehavior** intercepta comandos antes de la ejecución
- **FluentValidation** para reglas de negocio declarativas
- **ValidationException** personalizada para errores estructurados
- **ExceptionHandlingMiddleware** para respuestas consistentes

## 📊 Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API Controller
    participant MW as Exception Middleware
    participant M as MediatR
    participant LB as Logging Behavior
    participant VB as Validation Behavior
    participant V as FluentValidator
    participant CH as Command Handler

    Note over C,CH: FLUJO DE COMMAND CON ERROR DE VALIDACIÓN

    C->>+API: POST /api/orders (Invalid CreateOrderRequest)
    Note over C: Datos inválidos:<br/>• Email malformado<br/>• Cantidad negativa<br/>• Campo requerido faltante
    
    API->>API: Map Request → CreateOrderCommand
    
    API->>+MW: Execute Pipeline
    MW->>+M: Send(CreateOrderCommand)
    
    Note over M,VB: MEDIATOR PIPELINE
    M->>+LB: Handle Request
    LB->>LB: Log Request Started
    
    LB->>+VB: Continue Pipeline
    Note over VB: ValidationBehavior<CreateOrderCommand>
    
    VB->>+V: Validate(CreateOrderCommand)
    Note over V: FluentValidation Rules
    
    V->>V: Validate Email Format
    V->>V: ❌ Invalid Email
    
    V->>V: Validate OrderLines.Quantity > 0
    V->>V: ❌ Negative Quantity
    
    V->>V: Validate Required Fields
    V->>V: ❌ Missing City
    
    V-->>-VB: ValidationResult (HasErrors: true)
    
    Note over VB: VALIDATION FAILURE HANDLING
    VB->>VB: Collect ValidationErrors
    VB->>VB: Create ValidationError List
    
    VB->>VB: ❌ throw ValidationException
    Note over VB: ValidationException(<br/>List<ValidationError>)
    
    VB--X-LB: ValidationException
    LB--X-M: ValidationException  
    M--X-MW: ValidationException
    
    Note over MW: EXCEPTION HANDLING MIDDLEWARE
    MW->>MW: Catch ValidationException
    MW->>MW: Log Exception Details
    MW->>MW: GetExceptionDetails(ValidationException)
    
    MW->>MW: Create ProblemDetails
    Note over MW: Status: 400 Bad Request<br/>Type: ValidationFailure<br/>Title: Validation error<br/>Errors: List<ValidationError>
    
    MW->>MW: Set Response Status = 400
    MW->>MW: WriteAsJsonAsync(ProblemDetails)
    
    MW-->>-API: 400 Bad Request Response
    API-->>-C: 400 Bad Request + Validation Errors JSON

    Note over C,CH: ❌ VALIDACIÓN FALLIDA - NO SE EJECUTA HANDLER

    rect rgb(255, 235, 238)
        Note over C: RESPUESTA DE ERROR<br/>{<br/>  "status": 400,<br/>  "type": "ValidationFailure",<br/>  "title": "Validation error",<br/>  "detail": "One or more validation errors has occurred",<br/>  "errors": [<br/>    {<br/>      "propertyName": "Email",<br/>      "errorMessage": "Invalid email format"<br/>    },<br/>    {<br/>      "propertyName": "OrderLines[0].Quantity",<br/>      "errorMessage": "Quantity must be greater than 0"<br/>    },<br/>    {<br/>      "propertyName": "City",<br/>      "errorMessage": "City is required"<br/>    }<br/>  ]<br/>}
    end

```

## 🔍 Puntos Clave del Flujo de Error

### 1. **Validación Temprana**

- La validación ocurre **antes** de la ejecución del handler
- **ValidationBehavior** intercepta en el pipeline de MediatR
- Evita procesamiento innecesario si los datos son inválidos

### 2. **FluentValidation Detallada**

- Reglas declarativas y reutilizables
- **Múltiples errores** capturados simultáneamente
- Mensajes específicos y localizables
- Validación de objetos complejos y anidados

### 3. **Manejo Estructurado de Excepciones**

- **ValidationException** personalizada con lista de errores
- **ExceptionHandlingMiddleware** centraliza el manejo
- Respuesta consistente en formato **Problem Details (RFC 7807)**

### 4. **Respuesta Cliente-Amigable**

- **HTTP 400 Bad Request** apropiado
- **JSON estructurado** con detalles específicos
- Campo `errors` con lista detallada de problemas
- Información suficiente para corrección por parte del cliente

### 5. **No Ejecución del Handler**

- El **Command Handler nunca se ejecuta**
- **No se accede a repositorios** ni base de datos
- **No se inician transacciones** innecesarias
- Optimización de recursos y performance

## 🛠️ Reglas de Validación Típicas

### CreateOrderCommand Validator

```csharp
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty().WithMessage("Point of Sale phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.OrderLines)
            .NotEmpty().WithMessage("At least one order line is required");

        RuleForEach(x => x.OrderLines).SetValidator(new CreateOrderLineCommandValidator());
    }
}
```

## 📚 Casos de Error Representados

- **Campos requeridos faltantes**
- **Formatos inválidos** (email, teléfono, fechas)
- **Valores fuera de rango** (cantidades negativas, precios inválidos)
- **Longitudes excesivas** de texto
- **Relaciones inexistentes** (IDs no válidos)
- **Reglas de negocio** específicas del dominio

## ⚡ Beneficios del Approach

- ✅ **Fail Fast** - errores detectados tempranamente
- ✅ **Consistencia** en respuestas de error
- ✅ **Performance** - no procesa datos inválidos
- ✅ **User Experience** - errores claros y accionables
- ✅ **Maintainability** - validaciones centralizadas
- ✅ **Testability** - rules fáciles de probar unitariamente

