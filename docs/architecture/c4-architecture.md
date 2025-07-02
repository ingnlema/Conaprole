# 🏗️ Arquitectura C4 - Conaprole Orders API

> **Propósito**: Documentar la arquitectura del sistema usando el modelo C4  
> **Audiencia**: Arquitectos, Desarrolladores Senior, Stakeholders  
> **Prerrequisitos**: Conocimientos básicos de arquitectura de software

## 🎯 Objetivos

Proporcionar vistas arquitectónicas de alto nivel del sistema Conaprole Orders API siguiendo el modelo C4:

- **Contexto del Sistema**: Relación con usuarios y sistemas externos
- **Contenedores**: Componentes principales y tecnologías
- **Componentes**: Estructura interna de la aplicación

---

## 📊 Nivel 1: Contexto del Sistema

```mermaid
C4Context
    title Contexto del Sistema - Conaprole Orders API

    Person(pos_user, "Punto de Venta", "Usuario que gestiona pedidos desde puntos de venta")
    Person(distributor, "Distribuidor", "Distribuidor que gestiona entregas y productos")
    Person(admin, "Administrador", "Administrator del sistema")
    
    System(orders_api, "Conaprole Orders API", "Sistema de gestión de pedidos de productos lácteos")
    
    System_Ext(keycloak, "Keycloak", "Sistema de autenticación y gestión de identidades")
    SystemDb_Ext(postgres, "PostgreSQL", "Base de datos principal")
    
    Rel(pos_user, orders_api, "Crea y consulta pedidos", "HTTPS/REST")
    Rel(distributor, orders_api, "Gestiona entregas y productos", "HTTPS/REST")
    Rel(admin, orders_api, "Administra usuarios y permisos", "HTTPS/REST")
    
    Rel(orders_api, keycloak, "Autentica usuarios", "HTTPS")
    Rel(orders_api, postgres, "Almacena datos", "TCP/SQL")
```

### Actores Principales

| Actor | Responsabilidades | Permisos |
|-------|------------------|----------|
| **Punto de Venta** | Crear pedidos, consultar productos | `orders:write`, `products:read` |
| **Distribuidor** | Gestionar entregas, actualizar inventario | `orders:read`, `distributors:write` |
| **Administrador** | Configurar sistema, gestionar usuarios | `admin:access`, `users:write` |

---

## 📦 Nivel 2: Contenedores

```mermaid
C4Container
    title Contenedores - Conaprole Orders API

    Person(user, "Usuario", "Punto de Venta, Distribuidor, Admin")
    
    Container_Boundary(system, "Conaprole Orders System") {
        Container(api, "Orders API", ".NET 8, ASP.NET Core", "API REST que expone funcionalidad de negocio")
        Container(app, "Application Layer", ".NET 8, MediatR", "Lógica de aplicación y casos de uso")
        Container(domain, "Domain Layer", ".NET 8, DDD", "Modelos de dominio y reglas de negocio")
        Container(infra, "Infrastructure Layer", ".NET 8, EF Core", "Acceso a datos y servicios externos")
    }
    
    ContainerDb(db, "PostgreSQL Database", "PostgreSQL 15", "Almacena pedidos, usuarios, productos")
    Container_Ext(auth, "Keycloak", "OpenID Connect", "Autenticación y gestión de identidades")
    
    Rel(user, api, "Hace peticiones HTTP", "HTTPS/REST/JSON")
    Rel(api, app, "Ejecuta casos de uso", "C# Calls")
    Rel(app, domain, "Usa reglas de negocio", "C# Calls")
    Rel(app, infra, "Persiste datos", "C# Calls")
    Rel(infra, db, "Consultas SQL", "TCP/SQL")
    Rel(api, auth, "Valida tokens JWT", "HTTPS/OIDC")
```

### Tecnologías por Contenedor

| Contenedor | Tecnología Principal | Responsabilidad |
|------------|---------------------|-----------------|
| **Orders API** | ASP.NET Core 8.0 | Endpoints REST, autorización, documentación |
| **Application** | MediatR, FluentValidation | CQRS, validaciones, orquestación |
| **Domain** | .NET 8, DDD | Entidades, agregados, reglas de negocio |
| **Infrastructure** | Entity Framework Core | Persistencia, integración externa |

---

## 🔧 Nivel 3: Componentes (API Layer)

```mermaid
C4Component
    title Componentes - Orders API Layer

    Container_Boundary(api, "Orders API") {
        Component(controllers, "Controllers", "ASP.NET Core MVC", "Endpoints REST por dominio")
        Component(auth, "Authorization", "Custom Attributes", "Verificación de permisos")
        Component(swagger, "OpenAPI/Swagger", "Swashbuckle", "Documentación automática")
        Component(middleware, "Middleware", "ASP.NET Core", "Cross-cutting concerns")
        Component(dtos, "DTOs", "Records/Classes", "Objetos de transferencia")
    }
    
    Container(app_layer, "Application Layer", "MediatR", "Casos de uso")
    Container_Ext(keycloak, "Keycloak", "OAuth2/OIDC", "Proveedor de identidad")
    ContainerDb(postgres, "PostgreSQL", "Database", "Almacén de permisos")
    
    Rel(controllers, auth, "Valida permisos", "Attributes")
    Rel(controllers, app_layer, "Ejecuta comandos/queries", "MediatR")
    Rel(auth, postgres, "Consulta permisos", "EF Core")
    Rel(controllers, keycloak, "Valida JWT", "Bearer Token")
    Rel(swagger, controllers, "Documenta endpoints", "Reflection")
```

### Patrones de Componentes

#### Controllers por Dominio

```csharp
// Ejemplo: src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    [HasPermission(Permissions.OrdersRead)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
```

#### Authorization Handler

```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/PermissionAuthorizationHandler.cs
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Verificar permisos en base de datos
        var hasPermission = await CheckUserPermission(userId, requirement.Permission);
        if (hasPermission) context.Succeed(requirement);
    }
}
```

---

## 🔄 Flujos de Autorización

### Flujo de Autenticación y Autorización

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant Auth as Authorization Handler
    participant DB as PostgreSQL
    participant KC as Keycloak

    Client->>API: HTTP Request + JWT Token
    API->>KC: Validate JWT Token
    KC-->>API: Token Valid (sub, username)
    API->>Auth: Check Permission
    Auth->>DB: Query User Permissions
    DB-->>Auth: Permission Result
    Auth-->>API: Authorization Result
    
    alt Authorized
        API->>API: Execute Business Logic
        API-->>Client: Success Response
    else Not Authorized
        API-->>Client: 403 Forbidden
    end
```

---

## 🔗 Referencias

- [Clean Architecture](clean-architecture.md)
- [CQRS & MediatR](cqrs-mediator.md)
- [Authorization Details](../security/authorization.md)
- [API Design Patterns](api-design.md)

---

> **Última verificación**: 2025-07-02  
> **Commit SHA**: 20c7d61  
> **Estado**: ✅ Verificado
