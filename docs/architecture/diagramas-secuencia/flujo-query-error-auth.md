# 🔒 Flujo de Query con Error de Autorización

## 📋 Descripción

Este diagrama representa el flujo alternativo cuando una query falla debido a problemas de autorización. Muestra cómo el sistema valida permisos y maneja accesos no autorizados de manera segura y consistente.

## 🏗️ Arquitectura de Seguridad

- **JWT Authentication Middleware** para validación de tokens
- **Claims Transformation** para enriquecimiento de permisos
- **Permission-based Authorization** con atributos declarativos
- **Authorization Handlers** para verificación de permisos específicos

## 📊 Diagrama de Secuencia

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as API Controller
    participant MW as Exception Middleware
    participant AUTH as Auth Middleware
    participant CT as Claims Transform
    participant AH as Authorization Handler
    participant AS as Auth Service
    participant KC as Keycloak
    participant DB as Database
    participant M as MediatR

    Note over C,DB: FLUJO DE QUERY CON ERROR DE AUTORIZACIÓN

    C->>+API: GET /api/orders/sensitive-data (JWT Header)
    Note over C: JWT Token:<br/>• Usuario: regular@test.com<br/>• Rol: Registered<br/>• Permisos limitados
    
    API->>+MW: Execute Pipeline
    MW->>+AUTH: Validate Authentication & Authorization
    
    Note over AUTH: JWT AUTHENTICATION
    AUTH->>AUTH: Extract JWT from Authorization Header
    AUTH->>+KC: Validate JWT Token
    KC->>KC: Verify Signature & Expiration
    KC-->>-AUTH: ✅ Token Valid, IdentityId: user123
    
    AUTH->>+CT: Transform Claims
    Note over CT: Claims Transformation Service
    
    CT->>+AS: GetRolesForUserAsync(user123)
    AS->>+DB: SELECT roles FROM user_roles WHERE user_id = ?
    DB-->>-AS: [{ name: "Registered", id: 1 }]
    AS-->>-CT: UserRolesResponse
    
    CT->>CT: Enrich ClaimsPrincipal with Roles
    CT-->>-AUTH: Claims with roles["Registered"]
    
    Note over AUTH: PERMISSION AUTHORIZATION
    AUTH->>AUTH: Check [HasPermission("orders:admin")] Attribute
    AUTH->>+AH: HandleRequirementAsync("orders:admin")
    
    AH->>+AS: GetPermissionsForUserAsync(user123)
    AS->>+DB: SELECT p.name FROM permissions p<br/>JOIN role_permissions rp ON p.id = rp.permission_id<br/>JOIN user_roles ur ON rp.role_id = ur.role_id<br/>WHERE ur.user_id = ?
    DB-->>-AS: ["orders:read", "users:read"]
    AS-->>-AH: HashSet<string>{"orders:read", "users:read"}
    
    AH->>AH: Check if "orders:admin" in permissions
    AH->>AH: ❌ Permission NOT Found
    
    AH-->>-AUTH: AuthorizationResult.Fail("Insufficient permissions")
    
    AUTH->>AUTH: ❌ Authorization Failed
    AUTH->>AUTH: Create 403 Forbidden Response
    
    AUTH-->>-MW: 403 Forbidden Response
    MW-->>-API: 403 Forbidden Response
    API-->>-C: 403 Forbidden + Error Details

    Note over C,M: ❌ ACCESO DENEGADO - MEDIATOR NUNCA EJECUTADO

    rect rgb(255, 235, 238)
        Note over C: RESPUESTA DE ERROR<br/>{<br/>  "status": 403,<br/>  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",<br/>  "title": "Forbidden",<br/>  "detail": "You do not have permission to access this resource",<br/>  "instance": "/api/orders/sensitive-data"<br/>}
    end

    classDef error fill:#f8d7da,stroke:#721c24
    classDef security fill:#e2e3e5,stroke:#383d41
    classDef process fill:#d1ecf1,stroke:#0c5460
    classDef external fill:#fff3cd,stroke:#856404
    classDef data fill:#f0f9ff,stroke:#0c4a6e

    class C error
    class AUTH,CT,AH security
    class MW,API process
    class KC external
    class AS,DB data
    class M error
```

## 🔍 Puntos Clave del Flujo de Autorización

### 1. **Autenticación JWT Exitosa**
- Token válido con firma verificada
- Extracción correcta del `IdentityId`
- Usuario identificado en el sistema

### 2. **Enriquecimiento de Claims**
- **Claims Transformation** obtiene roles del usuario
- Consulta a base de datos local para roles específicos
- ClaimsPrincipal enriquecido con información de autorización

### 3. **Verificación de Permisos Específicos**
- **HasPermission Attribute** requiere permiso específico
- **Authorization Handler** verifica permisos del usuario
- Comparación entre permisos requeridos vs. permisos disponibles

### 4. **Fallo de Autorización**
- Usuario **autenticado** pero **no autorizado**
- Respuesta **403 Forbidden** (no 401 Unauthorized)
- **Información mínima** revelada por seguridad

### 5. **Terminación Temprana del Pipeline**
- **MediatR nunca se ejecuta**
- **Query Handler no se invoca**
- **Base de datos no consultada** para los datos solicitados

## 🛡️ Modelo de Permisos

### Estructura de Permisos
```
Usuario "regular@test.com"
├── Rol: "Registered"
├── Permisos Disponibles:
│   ├── "orders:read"      ✅
│   ├── "users:read"       ✅
│   └── "orders:admin"     ❌ (REQUERIDO)
└── Resultado: 403 Forbidden
```

### Casos de Permisos Típicos
```csharp
[HasPermission("orders:read")]    // ✅ Usuario tiene permiso
[HasPermission("orders:write")]   // ❌ Usuario no tiene permiso
[HasPermission("orders:admin")]   // ❌ Usuario no tiene permiso
[HasPermission("users:admin")]    // ❌ Usuario no tiene permiso
```

## 🔐 Diferencias entre 401 vs 403

| Código | Significado | Cuándo usar | Acción del Cliente |
|--------|-------------|-------------|-------------------|
| **401 Unauthorized** | No autenticado | JWT inválido, expirado, o ausente | Reautenticarse |
| **403 Forbidden** | Autenticado pero sin permisos | JWT válido pero permisos insuficientes | Solicitar permisos |

## 📚 Escenarios de Autorización

### ✅ Casos de Éxito
- Usuario Admin accede a cualquier recurso
- Usuario con permiso específico accede a recurso correspondiente
- Usuario accede a sus propios datos

### ❌ Casos de Fallo
- **Usuario regular** intenta acceder a datos administrativos
- **Usuario sin rol** intenta cualquier operación
- **Usuario de Distribuidor A** intenta acceder a datos de Distributor B
- **Token expirado** durante la verificación

## ⚡ Optimizaciones de Seguridad

### 1. **Caching de Permisos**
```csharp
// Los permisos pueden ser cacheados por usuario
[MemoryCache("user-permissions-{identityId}", Duration = "00:15:00")]
```

### 2. **Minimización de Information Leakage**
```json
// ❌ NO hacer: Información detallada
{
  "error": "User john@doe.com does not have orders:admin permission"
}

// ✅ Hacer: Información mínima
{
  "status": 403,
  "title": "Forbidden",
  "detail": "You do not have permission to access this resource"
}
```

### 3. **Logging de Seguridad**
```csharp
_logger.LogWarning("Authorization failed for user {UserId} attempting to access {Resource} with permission {Permission}",
    user.IdentityId, context.Resource, requiredPermission);
```

## 🔄 Comparación con Query Exitosa

| Aspecto | Query Exitosa | Query con Error Auth |
|---------|---------------|----------------------|
| **JWT Validation** | ✅ Válido | ✅ Válido |
| **Claims Transform** | ✅ Roles cargados | ✅ Roles cargados |
| **Permission Check** | ✅ Permiso encontrado | ❌ Permiso faltante |
| **Pipeline Execution** | Completo hasta DB | Terminado en Auth |
| **Response Status** | 200 OK | 403 Forbidden |
| **Data Exposure** | Datos solicitados | Sin datos |

## 🎯 Beneficios del Modelo

- ✅ **Principio de menor privilegio**
- ✅ **Autorización granular** por recurso y acción
- ✅ **Fail secure** - por defecto denegar
- ✅ **Auditabilidad** completa de accesos
- ✅ **Escalabilidad** del modelo de permisos
- ✅ **Separation of concerns** - auth separado de business logic