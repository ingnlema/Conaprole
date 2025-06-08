# 🖼️ Diagramas de Seguridad

Esta sección contiene diagramas técnicos que ilustran la arquitectura y flujos de seguridad del sistema **Conaprole Orders**.

## Diagrama de Arquitectura General

```mermaid
graph TB
    subgraph "Cliente"
        WEB[Web Browser/App]
        API_CLIENT[API Client]
    end

    subgraph "API Gateway/Load Balancer"
        LB[Load Balancer]
    end

    subgraph "Conaprole Orders API"
        MIDDLEWARE[Authentication Middleware]
        CONTROLLER[Controllers]
        AUTH_SERVICE[Authorization Service]
        USER_CONTEXT[User Context]
    end

    subgraph "Keycloak Identity Provider"
        KC_AUTH[Auth Client]
        KC_ADMIN[Admin Client]
        KC_REALM[Conaprole Realm]
    end

    subgraph "Database"
        DB_USERS[Users Table]
        DB_ROLES[Roles Table]
        DB_PERMISSIONS[Permissions Table]
        DB_ROLE_PERM[RolePermissions Table]
    end

    WEB --> LB
    API_CLIENT --> LB
    LB --> MIDDLEWARE
    
    MIDDLEWARE --> KC_AUTH
    MIDDLEWARE --> AUTH_SERVICE
    MIDDLEWARE --> CONTROLLER
    
    CONTROLLER --> USER_CONTEXT
    CONTROLLER --> AUTH_SERVICE
    
    AUTH_SERVICE --> DB_USERS
    AUTH_SERVICE --> DB_ROLES
    AUTH_SERVICE --> DB_PERMISSIONS
    
    CONTROLLER --> KC_ADMIN
    
    KC_AUTH --> KC_REALM
    KC_ADMIN --> KC_REALM
    
    DB_USERS --> DB_ROLE_PERM
    DB_ROLES --> DB_ROLE_PERM
    DB_PERMISSIONS --> DB_ROLE_PERM

    classDef client fill:#e1f5fe
    classDef api fill:#f3e5f5
    classDef keycloak fill:#fff3e0
    classDef database fill:#e8f5e8

    class WEB,API_CLIENT client
    class MIDDLEWARE,CONTROLLER,AUTH_SERVICE,USER_CONTEXT api
    class KC_AUTH,KC_ADMIN,KC_REALM keycloak
    class DB_USERS,DB_ROLES,DB_PERMISSIONS,DB_ROLE_PERM database
```

## Flujo Completo de Autenticación y Autorización

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as Conaprole API
    participant MW as Auth Middleware
    participant CT as Claims Transform
    participant AH as Auth Handler
    participant AS as Auth Service
    participant KC as Keycloak
    participant DB as Database

    Note over C,DB: 1. REGISTRO DE USUARIO
    C->>API: POST /users/register
    API->>KC: Crear usuario
    KC-->>API: IdentityId
    API->>DB: Guardar User + Role "Registered"
    API-->>C: Usuario creado

    Note over C,DB: 2. LOGIN
    C->>API: POST /users/login
    API->>KC: Validar credenciales
    KC-->>API: JWT Token
    API-->>C: JWT Token

    Note over C,DB: 3. REQUEST AUTENTICADO
    C->>API: GET /users/me (JWT Header)
    API->>MW: Validar JWT
    MW->>KC: Verificar firma/expiración
    KC-->>MW: Token válido
    
    MW->>CT: Transform Claims
    CT->>AS: GetRolesForUserAsync(identityId)
    AS->>DB: Query roles por IdentityId
    DB-->>AS: User roles
    AS-->>CT: UserRolesResponse
    CT->>CT: Enriquecer ClaimsPrincipal
    CT-->>MW: Claims con roles

    Note over C,DB: 4. VERIFICACIÓN DE PERMISOS
    MW->>AH: HandleRequirementAsync("users:read")
    AH->>AS: GetPermissionsForUserAsync(identityId)
    AS->>DB: Query permisos por roles
    DB-->>AS: Lista permisos
    AS-->>AH: HashSet<string>
    AH->>AH: ¿permissions.Contains("users:read")?
    
    alt Autorizado
        AH-->>MW: Success
        MW-->>API: Continuar
        API-->>C: Respuesta autorizada
    else No autorizado
        AH-->>MW: Fail
        MW-->>C: 403 Forbidden
    end
```

## Diagrama de Componentes de Seguridad

```mermaid
graph TB
    subgraph "Presentation Layer"
        CTRL[Controllers]
        ATTR[HasPermission Attributes]
    end

    subgraph "Authentication Layer"
        JWT_MW[JWT Middleware]
        JWT_SERVICE[JWT Service]
        AUTH_SERVICE[Authentication Service]
        USER_CONTEXT[User Context]
    end

    subgraph "Authorization Layer"
        CLAIMS_TRANSFORM[Claims Transformation]
        AUTH_HANDLER[Permission Handler]
        POLICY_PROVIDER[Policy Provider]
        AUTHZ_SERVICE[Authorization Service]
    end

    subgraph "Domain Layer"
        USER[User Entity]
        ROLE[Role Entity]
        PERMISSION[Permission Entity]
    end

    subgraph "Infrastructure Layer"
        KC_CLIENT[Keycloak Clients]
        DB_CONTEXT[DB Context]
        HTTP_CLIENT[HTTP Clients]
    end

    CTRL --> ATTR
    ATTR --> POLICY_PROVIDER
    
    JWT_MW --> JWT_SERVICE
    JWT_MW --> CLAIMS_TRANSFORM
    
    CLAIMS_TRANSFORM --> AUTHZ_SERVICE
    
    POLICY_PROVIDER --> AUTH_HANDLER
    AUTH_HANDLER --> AUTHZ_SERVICE
    
    AUTH_SERVICE --> KC_CLIENT
    JWT_SERVICE --> KC_CLIENT
    
    AUTHZ_SERVICE --> DB_CONTEXT
    USER_CONTEXT --> JWT_MW
    
    AUTHZ_SERVICE --> USER
    AUTHZ_SERVICE --> ROLE
    AUTHZ_SERVICE --> PERMISSION
    
    KC_CLIENT --> HTTP_CLIENT
    DB_CONTEXT --> USER
    DB_CONTEXT --> ROLE
    DB_CONTEXT --> PERMISSION

    classDef presentation fill:#ffebee
    classDef auth fill:#e3f2fd
    classDef authz fill:#f1f8e9
    classDef domain fill:#fff8e1
    classDef infra fill:#fce4ec

    class CTRL,ATTR presentation
    class JWT_MW,JWT_SERVICE,AUTH_SERVICE,USER_CONTEXT auth
    class CLAIMS_TRANSFORM,AUTH_HANDLER,POLICY_PROVIDER,AUTHZ_SERVICE authz
    class USER,ROLE,PERMISSION domain
    class KC_CLIENT,DB_CONTEXT,HTTP_CLIENT infra
```

## Modelo de Datos de Seguridad

```mermaid
erDiagram
    Users {
        uuid Id PK
        string FirstName
        string LastName
        string Email
        string IdentityId UK "Keycloak ID"
        uuid DistributorId FK
        datetime CreatedAt
        datetime UpdatedAt
    }

    Roles {
        int Id PK
        string Name UK "Ej: Registered, Admin"
        string Description
        datetime CreatedAt
    }

    Permissions {
        int Id PK
        string Name UK "Ej: users:read, orders:write"
        string Description
        string Resource "Ej: users, orders"
        string Action "Ej: read, write, delete"
        datetime CreatedAt
    }

    UserRoles {
        uuid UserId PK,FK
        int RoleId PK,FK
        datetime AssignedAt
    }

    RolePermissions {
        int RoleId PK,FK
        int PermissionId PK,FK
        datetime GrantedAt
    }

    Distributors {
        uuid Id PK
        string Name
        string PhoneNumber
        string Address
    }

    Users ||--o{ UserRoles : "has"
    Roles ||--o{ UserRoles : "assigned to"
    Roles ||--o{ RolePermissions : "has"
    Permissions ||--o{ RolePermissions : "granted by"
    Users }o--|| Distributors : "belongs to"
```

## Flujo de Verificación de Permisos

```mermaid
flowchart TD
    START([Request con JWT]) --> VALIDATE{¿JWT válido?}
    
    VALIDATE -->|No| RETURN_401[401 Unauthorized]
    VALIDATE -->|Sí| EXTRACT[Extraer IdentityId]
    
    EXTRACT --> CHECK_CLAIMS{¿Claims ya<br/>enriquecidos?}
    
    CHECK_CLAIMS -->|No| LOAD_ROLES[Cargar roles de DB]
    CHECK_CLAIMS -->|Sí| CHECK_PERM[Verificar permiso]
    
    LOAD_ROLES --> ENRICH_CLAIMS[Enriquecer ClaimsPrincipal]
    ENRICH_CLAIMS --> CHECK_PERM
    
    CHECK_PERM --> LOAD_PERMS[Cargar permisos de roles]
    LOAD_PERMS --> HAS_PERM{¿Tiene permiso<br/>requerido?}
    
    HAS_PERM -->|Sí| ALLOW[Permitir acceso]
    HAS_PERM -->|No| RETURN_403[403 Forbidden]
    
    ALLOW --> EXECUTE[Ejecutar controlador]
    EXECUTE --> RESPONSE[Retornar respuesta]
    
    RETURN_401 --> END([Fin])
    RETURN_403 --> END
    RESPONSE --> END

    classDef success fill:#d4edda
    classDef error fill:#f8d7da
    classDef process fill:#d1ecf1
    classDef decision fill:#fff3cd

    class ALLOW,EXECUTE,RESPONSE success
    class RETURN_401,RETURN_403 error
    class EXTRACT,LOAD_ROLES,ENRICH_CLAIMS,LOAD_PERMS process
    class VALIDATE,CHECK_CLAIMS,HAS_PERM decision
```

## Configuración de Keycloak

```mermaid
graph TB
    subgraph "Keycloak Realm: Conaprole"
        subgraph "Clients"
            AUTH_CLIENT[conaprole-auth-client<br/>🔐 User Authentication]
            ADMIN_CLIENT[conaprole-admin-client<br/>🛠️ Admin Operations]
        end
        
        subgraph "Users"
            KC_USERS[Keycloak Users<br/>📧 Email/Password]
        end
        
        subgraph "Configuration"
            REALM_SETTINGS[Realm Settings<br/>🔧 Token Policies]
            CLIENT_SCOPES[Client Scopes<br/>📋 OpenID/Email]
        end
    end

    subgraph "Application Database"
        APP_USERS[Users Table<br/>🏷️ IdentityId Mapping]
        APP_ROLES[Roles & Permissions<br/>🔑 Business Logic]
    end

    AUTH_CLIENT --> KC_USERS
    ADMIN_CLIENT --> KC_USERS
    
    KC_USERS -.->|IdentityId| APP_USERS
    APP_USERS --> APP_ROLES
    
    REALM_SETTINGS --> AUTH_CLIENT
    REALM_SETTINGS --> ADMIN_CLIENT
    
    CLIENT_SCOPES --> AUTH_CLIENT

    classDef keycloak fill:#fff3e0
    classDef database fill:#e8f5e8
    classDef connection stroke-dasharray: 5 5

    class AUTH_CLIENT,ADMIN_CLIENT,KC_USERS,REALM_SETTINGS,CLIENT_SCOPES keycloak
    class APP_USERS,APP_ROLES database
```

## Diagrama de Despliegue

```mermaid
graph TB
    subgraph "DMZ/Internet"
        LB[Load Balancer<br/>🌐 HTTPS Termination]
    end

    subgraph "Application Tier"
        API1[Conaprole API Instance 1<br/>🚀 Port 8001]
        API2[Conaprole API Instance 2<br/>🚀 Port 8002]
        API3[Conaprole API Instance N<br/>🚀 Port 800N]
    end

    subgraph "Identity Tier"
        KC1[Keycloak Instance 1<br/>🔐 Port 8080]
        KC2[Keycloak Instance 2<br/>🔐 Port 8080]
    end

    subgraph "Data Tier"
        DB_APP[(Application DB<br/>🗃️ PostgreSQL)]
        DB_KC[(Keycloak DB<br/>🗃️ PostgreSQL)]
    end

    subgraph "Monitoring"
        LOGS[Centralized Logging<br/>📋 ELK Stack]
        METRICS[Metrics<br/>📊 Prometheus]
    end

    LB --> API1
    LB --> API2
    LB --> API3

    API1 --> KC1
    API1 --> KC2
    API2 --> KC1
    API2 --> KC2
    API3 --> KC1
    API3 --> KC2

    API1 --> DB_APP
    API2 --> DB_APP
    API3 --> DB_APP

    KC1 --> DB_KC
    KC2 --> DB_KC

    API1 --> LOGS
    API2 --> LOGS
    API3 --> LOGS
    KC1 --> LOGS
    KC2 --> LOGS

    API1 --> METRICS
    API2 --> METRICS
    API3 --> METRICS
    KC1 --> METRICS
    KC2 --> METRICS

    classDef dmz fill:#ffebee
    classDef app fill:#e3f2fd
    classDef identity fill:#fff3e0
    classDef data fill:#e8f5e8
    classDef monitoring fill:#f3e5f5

    class LB dmz
    class API1,API2,API3 app
    class KC1,KC2 identity
    class DB_APP,DB_KC data
    class LOGS,METRICS monitoring
```

## Matriz de Responsabilidades de Seguridad

```mermaid
graph LR
    subgraph "Responsabilidades por Capa"
        subgraph "Keycloak"
            KC_RESP[✅ Gestión de identidades<br/>✅ Autenticación de usuarios<br/>✅ Emisión de JWT tokens<br/>✅ Gestión de credenciales<br/>✅ Políticas de contraseñas<br/>✅ 2FA/MFA (futuro)]
        end
        
        subgraph "API Application"
            API_RESP[✅ Validación de JWT<br/>✅ Autorización por permisos<br/>✅ Transformación de claims<br/>✅ Lógica de negocio<br/>✅ Contexto de usuario<br/>✅ Auditoría de acciones]
        end
        
        subgraph "Database"
            DB_RESP[✅ Usuarios del dominio<br/>✅ Roles y permisos<br/>✅ Relaciones usuario-rol<br/>✅ Datos de negocio<br/>✅ Auditoría de cambios<br/>✅ Respaldos seguros]
        end
    end

    classDef keycloak fill:#fff3e0
    classDef api fill:#e3f2fd  
    classDef database fill:#e8f5e8

    class KC_RESP keycloak
    class API_RESP api
    class DB_RESP database
```

---

## Leyenda de Diagramas

### Símbolos Utilizados
- 🔐 Autenticación/Seguridad
- 🛠️ Administración/Gestión
- 📧 Comunicación/Email
- 🔧 Configuración
- 📋 Información/Datos
- 🔑 Permisos/Acceso
- 🏷️ Identificación/Mapeo
- 🌐 Red/Internet
- 🚀 Aplicación/Servicio
- 🗃️ Base de datos
- 📊 Métricas/Monitoreo

### Códigos de Color
- **Azul claro**: Componentes de autenticación
- **Verde claro**: Base de datos y persistencia
- **Naranja claro**: Keycloak/Identity Provider
- **Rosa claro**: Componentes de presentación
- **Gris claro**: Infraestructura y red

---

*Ver también: [Architecture](./architecture.md) | [Authentication](./authentication.md) | [Authorization](./authorization.md)*