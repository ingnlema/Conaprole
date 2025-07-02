# 🔄 Flujo Típico de Command (Escritura)

## 📋 Descripción

Este diagrama representa el flujo típico de procesamiento de un comando (operación de escritura) en la API Core de Conaprole Orders, desde la recepción de la solicitud HTTP hasta la persistencia de datos y respuesta al cliente.

## 🏗️ Arquitectura Aplicada

- **Clean Architecture** con separación de capas
- **CQRS** con MediatR para manejo de comandos
- **Pipeline Behaviors** para validación y logging
- **Domain-Driven Design** con agregados y repositorios

## 📊 Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API Controller
    participant MW as Exception Middleware
    participant M as MediatR
    participant VB as Validation Behavior
    participant LB as Logging Behavior
    participant CH as Command Handler
    participant R as Repository
    participant UOW as Unit of Work
    participant DB as Database
    participant DE as Domain Entity

    Note over C,DB: FLUJO TÍPICO DE COMMAND

    C->>+API: POST /api/orders (CreateOrderRequest)
    Note over API: Map Request → CreateOrderCommand
    
    API->>+MW: Execute Pipeline
    MW->>+M: Send(CreateOrderCommand)
    
    Note over M,CH: MEDIATOR PIPELINE
    M->>+LB: Handle Request
    LB->>LB: Log Request Started
    
    LB->>+VB: Continue Pipeline
    VB->>VB: Validate Command
    Note over VB: FluentValidation Rules
    VB->>VB: ✅ Validation Passed
    
    VB->>+CH: Handle Command
    Note over CH: CreateOrderCommandHandler
    
    Note over CH,DB: BUSINESS LOGIC EXECUTION
    CH->>+R: GetByPhoneNumberAsync(PointOfSale)
    R->>+DB: SELECT from point_of_sale
    DB-->>-R: PointOfSale Entity
    R-->>-CH: PointOfSale
    
    CH->>+R: GetByPhoneNumberAsync(Distributor)
    R->>+DB: SELECT from distributor
    DB-->>-R: Distributor Entity
    R-->>-CH: Distributor
    
    Note over CH,DE: DOMAIN LOGIC
    CH->>+DE: Create Order Aggregate
    DE->>DE: Apply Business Rules
    DE->>DE: Create OrderLines
    DE-->>-CH: Order Entity
    
    CH->>+R: AddAsync(Order)
    R->>R: Track Entity
    R-->>-CH: void
    
    CH->>+UOW: SaveChangesAsync()
    UOW->>+DB: BEGIN TRANSACTION
    UOW->>DB: INSERT INTO orders
    UOW->>DB: INSERT INTO order_lines
    UOW->>DB: COMMIT
    DB-->>-UOW: Success
    UOW-->>-CH: Success
    
    CH-->>-VB: Result<Guid>(OrderId)
    VB-->>-LB: Result<Guid>
    
    LB->>LB: Log Request Completed
    LB-->>-M: Result<Guid>
    M-->>-MW: Result<Guid>
    MW-->>-API: Result<Guid>
    
    Note over API: Generate 201 Created Response
    API->>API: CreatedAtAction(GetOrder, OrderId)
    API-->>-C: 201 Created + Location Header

    Note over C,DB: ✅ COMANDO EJECUTADO EXITOSAMENTE

    classDef success fill:#d4edda,stroke:#155724
    classDef process fill:#d1ecf1,stroke:#0c5460
    classDef domain fill:#fff3cd,stroke:#856404
    classDef data fill:#f8d7da,stroke:#721c24

    class C,API success
    class MW,M,VB,LB,CH process
    class DE,R domain
    class UOW,DB data
```

## 🔍 Puntos Clave del Flujo

### 1. **Recepción y Mapeo**

- El controller recibe el HTTP Request
- Se mapea el DTO a un Command específico
- Se invoca MediatR para procesar el comando

### 2. **Pipeline de Behaviors**

- **Logging Behavior**: Registra inicio y fin de la operación
- **Validation Behavior**: Ejecuta reglas de FluentValidation
- Solo si la validación pasa, se ejecuta el handler

### 3. **Ejecución del Command Handler**

- Carga entidades necesarias desde repositorios
- Aplica lógica de dominio y business rules
- Crea y configura el agregado correspondiente

### 4. **Persistencia Transaccional**

- Usa Unit of Work para transacciones
- Persiste cambios de forma atómica
- Maneja rollback automático en caso de error

### 5. **Respuesta Exitosa**

- Retorna 201 Created con el ID del recurso creado
- Incluye Location header para acceso al recurso

## 📚 Casos de Uso Representados

Este flujo es representativo de comandos como:

- `CreateOrderCommand`
- `UpdateOrderStatusCommand`
- `AddOrderLineCommand`
- `CreateDistributorCommand`

## ⚡ Características Arquitectónicas

- ✅ **Separación de responsabilidades** por capas
- ✅ **Inversión de dependencias** (DIP)
- ✅ **Transaccionalidad** garantizada
- ✅ **Validación centralizada** en pipeline
- ✅ **Logging** automático de operaciones
- ✅ **Manejo de errores** por middleware
