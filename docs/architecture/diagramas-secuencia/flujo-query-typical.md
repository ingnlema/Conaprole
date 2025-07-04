# 🔍 Flujo Típico de Query (Lectura)

## 📋 Descripción

Este diagrama representa el flujo típico de procesamiento de una query (operación de lectura) en la API Core de Conaprole Orders, optimizada para consultas eficientes de datos sin modificación de estado.

## 🏗️ Arquitectura Aplicada

- **CQRS** separando lecturas de escrituras
- **Query Handlers** especializados para lecturas
- **Dapper** para consultas SQL optimizadas
- **DTOs** específicos para respuestas
- **Pipeline simplificado** sin validaciones complejas

## 📊 Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API Controller
    participant MW as Exception Middleware
    participant AUTH as Auth Middleware
    participant M as MediatR
    participant LB as Logging Behavior
    participant QH as Query Handler
    participant SCF as SQL Connection Factory
    participant DB as Database
    participant DTO as Response DTOs

    Note over C,DB: FLUJO TÍPICO DE QUERY

    C->>+API: GET /api/orders/{id} (JWT Header)
    Note over API: Create GetOrderQuery
    
    API->>+MW: Execute Pipeline
    MW->>+AUTH: Validate Authentication
    
    Note over AUTH: JWT VALIDATION
    AUTH->>AUTH: Validate JWT Token
    AUTH->>AUTH: Extract Claims
    AUTH->>AUTH: ✅ Token Valid
    
    AUTH->>+M: Send(GetOrderQuery)
    
    Note over M,QH: MEDIATOR PIPELINE (Simplified for Queries)
    M->>+LB: Handle Request
    LB->>LB: Log Query Started
    
    LB->>+QH: Handle Query
    Note over QH: GetOrderQueryHandler
    
    Note over QH,DB: OPTIMIZED DATA ACCESS
    QH->>+SCF: CreateConnection()
    SCF-->>-QH: IDbConnection
    
    QH->>+DB: QueryMultipleAsync(SQL)
    Note over DB: Direct SQL Query<br/>SELECT o.*, pos.*, d.*<br/>FROM orders o<br/>JOIN point_of_sale pos<br/>JOIN distributor d
    DB-->>-QH: Raw Data Results
    
    QH->>+DB: QueryAsync(OrderLines SQL)
    Note over DB: SELECT ol.*, p.*<br/>FROM order_lines ol<br/>JOIN products p
    DB-->>-QH: OrderLines Data
    
    Note over QH,DTO: DATA PROJECTION
    QH->>+DTO: Map to OrderResponse
    DTO->>DTO: Project to DTOs
    DTO->>DTO: Group OrderLines
    DTO->>DTO: Create ProductResponse
    DTO-->>-QH: Structured OrderResponse
    
    QH-->>-LB: Result<OrderResponse>
    
    LB->>LB: Log Query Completed
    LB-->>-M: Result<OrderResponse>
    M-->>-AUTH: Result<OrderResponse>
    AUTH-->>-MW: Result<OrderResponse>
    MW-->>-API: Result<OrderResponse>
    
    Note over API: Check Result Status
    API->>API: result.IsSuccess ? Ok(value) : NotFound(error)
    API-->>-C: 200 OK + OrderResponse JSON

    Note over C,DB: ✅ QUERY EJECUTADA EXITOSAMENTE

```

## 🔍 Puntos Clave del Flujo

### 1. **Autenticación Simplificada**

- Validación de JWT token
- Extracción de claims básicos
- Sin verificación de permisos específicos (para este ejemplo)

### 2. **Pipeline Optimizado para Lecturas**

- **Sin Validation Behavior** (queries raramente necesitan validación compleja)
- **Logging Behavior** para trazabilidad
- Enfoque en performance y rapidez

### 3. **Acceso Directo a Datos**

- **SQL Connection Factory** para conexiones eficientes
- **Dapper** para mapeo rápido de resultados
- **Consultas SQL optimizadas** específicas para la necesidad

### 4. **Proyección de Datos**

- Mapeo directo a DTOs de respuesta
- **Sin entidades de dominio** (no se necesitan)
- Estructuración de datos para consumo del cliente

### 5. **Respuesta Optimizada**

- Status code apropiado (200 OK / 404 Not Found)
- JSON serializado directamente
- Headers de respuesta mínimos

## 📚 Casos de Uso Representados

Este flujo es representativo de queries como:

- `GetOrderQuery`
- `GetOrdersQuery`
- `GetDistributorsQuery`
- `GetAssignedPointsOfSaleQuery`

## ⚡ Diferencias con Commands

| Aspecto | Commands | Queries |
|---------|----------|---------|
| **Validación** | FluentValidation compleja | Validación mínima |
| **Dominio** | Entidades y agregados | DTOs directos |
| **Persistencia** | Unit of Work + Repositorios | SQL Connection + Dapper |
| **Transacciones** | Requeridas | No necesarias |
| **Caching** | No aplicable | Potencial para caché |
| **Performance** | Consistencia > Speed | Speed > Overhead |

## 🎯 Optimizaciones Aplicadas

- ✅ **Sin Unit of Work** - no se modifican datos
- ✅ **SQL directo** con Dapper para máxima eficiencia
- ✅ **Proyección específica** solo datos necesarios
- ✅ **Pipeline reducido** menos overhead
- ✅ **Connection pooling** gestionado automáticamente
- ✅ **Mapeo optimizado** a DTOs de lectura

## 🔒 Consideraciones de Seguridad

- JWT validation en middleware de autenticación
- Claims extraction para contexto de usuario
- Potencial para verificación de permisos de lectura
- Logging de accesos para auditoría
