# 🏗️ System Architecture - C4 Diagrams

## Purpose

This document provides comprehensive architectural diagrams following the C4 model to visualize the Conaprole Orders system at different levels of detail.

## Audience

- **Solution Architects** - System design and integration planning
- **Technical Leads** - Architecture reviews and decisions
- **Developers** - Understanding system structure and dependencies

## Prerequisites

- Basic understanding of the C4 architectural model
- Familiarity with the Conaprole Orders system

## C4 Level 1: System Context

```mermaid
C4Context
    title System Context Diagram - Conaprole Orders

    Person(user, "Usuario Final", "Distribuidor, Punto de Venta, Administrador")
    Person(admin, "Administrador", "Gestiona usuarios, productos y configuración")
    
    System(orders_api, "Conaprole Orders API", "Sistema de gestión de pedidos y productos")
    
    System_Ext(keycloak, "Keycloak", "Sistema de autenticación y autorización")
    System_Ext(client_apps, "Aplicaciones Cliente", "Web, Mobile, Desktop")
    SystemDb_Ext(postgresql, "PostgreSQL", "Base de datos principal")
    
    Rel(user, client_apps, "Usa")
    Rel(admin, client_apps, "Administra")
    Rel(client_apps, orders_api, "Consume API", "HTTPS/REST")
    Rel(orders_api, keycloak, "Autentica usuarios", "HTTPS/OIDC")
    Rel(orders_api, postgresql, "Lee/Escribe datos", "SQL")
    
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="2")
```

## C4 Level 2: Container Diagram

```mermaid
C4Container
    title Container Diagram - Conaprole Orders API

    Person(user, "Usuario", "Distribuidor/Punto de Venta/Admin")
    
    System_Boundary(orders_system, "Conaprole Orders System") {
        Container(api, "Web API", ".NET 8, ASP.NET Core", "Provee endpoints REST para gestión de pedidos")
        Container(domain, "Domain Layer", ".NET 8", "Lógica de negocio y entidades de dominio")
        Container(application, "Application Layer", ".NET 8, MediatR", "Casos de uso y servicios de aplicación")
        Container(infrastructure, "Infrastructure Layer", ".NET 8, EF Core", "Acceso a datos y servicios externos")
    }
    
    System_Ext(keycloak, "Keycloak", "Autenticación JWT")
    SystemDb(db, "PostgreSQL", "Base de datos principal")
    System_Ext(client, "Cliente Web/Mobile", "Aplicación cliente")
    
    Rel(user, client, "Usa")
    Rel(client, api, "Consume", "HTTPS/REST")
    Rel(api, application, "Llama")
    Rel(application, domain, "Usa")
    Rel(application, infrastructure, "Llama")
    Rel(infrastructure, db, "Lee/Escribe", "Entity Framework")
    Rel(api, keycloak, "Valida tokens", "JWT")
    
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

## C4 Level 3: Component Diagram - API Layer

```mermaid
C4Component
    title Component Diagram - API Layer

    Container(client, "Cliente", "Aplicación web/mobile")
    
    Container_Boundary(api, "API Layer") {
        Component(users_ctrl, "UsersController", "Controller", "Gestión de usuarios")
        Component(orders_ctrl, "OrdersController", "Controller", "Gestión de pedidos")
        Component(products_ctrl, "ProductsController", "Controller", "Gestión de productos")
        Component(auth_middleware, "Authentication Middleware", "Middleware", "Validación JWT")
        Component(authz_handler, "Authorization Handler", "Handler", "Verificación de permisos")
        Component(exception_middleware, "Exception Middleware", "Middleware", "Manejo de errores")
    }
    
    Container(application, "Application Layer", "Casos de uso")
    Container(keycloak, "Keycloak", "Servidor de identidad")
    
    Rel(client, auth_middleware, "HTTP Request")
    Rel(auth_middleware, authz_handler, "Valida permisos")
    Rel(authz_handler, users_ctrl, "Autoriza")
    Rel(authz_handler, orders_ctrl, "Autoriza")
    Rel(authz_handler, products_ctrl, "Autoriza")
    Rel(users_ctrl, application, "Llama casos de uso")
    Rel(orders_ctrl, application, "Llama casos de uso")
    Rel(products_ctrl, application, "Llama casos de uso")
    Rel(auth_middleware, keycloak, "Valida JWT")
    Rel(exception_middleware, client, "Error Response")
    
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Sequence Diagrams

### User Authentication Flow

```mermaid
sequenceDiagram
    participant C as Cliente
    participant A as API Gateway
    participant K as Keycloak
    participant D as Database
    
    C->>K: POST /auth/realms/conaprole/protocol/openid-connect/token
    K->>K: Validar credenciales
    K->>C: JWT Token + Refresh Token
    
    C->>A: GET /api/users/me (Authorization: Bearer {token})
    A->>A: Validar JWT signature
    A->>K: Verificar token (opcional)
    A->>D: Obtener permisos del usuario
    A->>C: Datos del usuario + permisos
```

### Order Creation Flow

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as Orders API
    participant Auth as Authorization
    participant App as Application
    participant D as Database
    
    C->>API: POST /api/orders (JWT Token)
    API->>Auth: Verificar permiso "orders:write"
    Auth->>D: Consultar permisos del usuario
    Auth->>API: ✅ Autorizado
    
    API->>App: CreateOrderCommand
    App->>App: Validar datos de negocio
    App->>D: Guardar orden
    App->>API: OrderCreatedResponse
    API->>C: 201 Created + Order Details
```

## Deployment Architecture

```mermaid
graph TB
    subgraph "Azure Container Apps"
        API[Conaprole Orders API]
    end
    
    subgraph "Azure Database"
        DB[(PostgreSQL)]
    end
    
    subgraph "Identity Provider"
        KC[Keycloak]
    end
    
    subgraph "Clients"
        WEB[Web App]
        MOB[Mobile App]
        API_CLIENT[API Client]
    end
    
    WEB --> API
    MOB --> API
    API_CLIENT --> API
    API --> KC
    API --> DB
    
    API -.->|HTTPS| WEB
    API -.->|HTTPS| MOB
    API -.->|HTTPS| API_CLIENT
```

## Security Architecture

```mermaid
graph LR
    subgraph "Authentication Layer"
        JWT[JWT Token Validation]
        KC[Keycloak Integration]
    end
    
    subgraph "Authorization Layer"
        PERM[Permission System]
        ROLES[Role-Based Access]
        ATTR[HasPermission Attribute]
    end
    
    subgraph "API Endpoints"
        USERS[Users API]
        ORDERS[Orders API]
        PRODUCTS[Products API]
    end
    
    JWT --> PERM
    KC --> JWT
    PERM --> ATTR
    ROLES --> PERM
    ATTR --> USERS
    ATTR --> ORDERS
    ATTR --> PRODUCTS
```

## Data Flow Architecture

```mermaid
flowchart TD
    subgraph Request
        CLIENT[Client Request] --> MIDDLEWARE[Authentication Middleware]
        MIDDLEWARE --> AUTHZ[Authorization Handler]
        AUTHZ --> CONTROLLER[Controller]
        CONTROLLER --> MEDIATOR[MediatR]
    end

    subgraph Command Flow
        MEDIATOR --> CMD_HANDLER[Command Handler]
        CMD_HANDLER --> DOMAIN[Domain Services]
        CMD_HANDLER --> REPO[Repository]
        REPO --> EF[Entity Framework]
        EF --> DB[(PostgreSQL)]

        DOMAIN --> EVENTS[Domain Events]
        EVENTS --> EVENT_HANDLERS[Event Handlers]

        CMD_HANDLER --> CMD_RESPONSE[Response DTO]
        CMD_RESPONSE --> CONTROLLER
    end

    subgraph Query Flow
        MEDIATOR --> QRY_HANDLER[Query Handler]
        QRY_HANDLER --> DAPPER[Dapper SQL Access]
        DAPPER --> DB

        QRY_HANDLER --> QRY_RESPONSE[Response DTO]
        QRY_RESPONSE --> CONTROLLER
    end

    CONTROLLER --> CLIENT

    %% Visual styles for clarity
    classDef command fill:#e3f2fd,stroke:#2196f3
    classDef query fill:#f3e5f5,stroke:#9c27b0
    classDef shared fill:#e0f7fa,stroke:#006064

    class CMD_HANDLER,DOMAIN,REPO,EF,EVENTS,EVENT_HANDLERS,CMD_RESPONSE command
    class QRY_HANDLER,DAPPER,QRY_RESPONSE query
    class CLIENT,MIDDLEWARE,AUTHZ,CONTROLLER,MEDIATOR,DB shared

```



*Last verified: 2025-01-02 - Commit: [architecture diagrams added]*
