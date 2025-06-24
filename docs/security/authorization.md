# 🛂 Autorización

## Sistema de Autorización Basado en Permisos

La aplicación **Conaprole Orders** implementa un sistema de autorización granular basado en **permisos específicos** en lugar de roles estáticos. Esto permite mayor flexibilidad y control de acceso.

## Modelo de Dominio

### Permission (Permisos)
```csharp
// src/Conaprole.Orders.Domain/Users/Permission.cs
public sealed class Permission
{
    // Permisos de Usuarios
    public static readonly Permission UsersRead = new(1, "users:read");
    public static readonly Permission UsersWrite = new(2, "users:write");
    
    // Permisos de Distribuidores
    public static readonly Permission DistributorsRead = new(3, "distributors:read");
    public static readonly Permission DistributorsWrite = new(4, "distributors:write");
    
    // Permisos de Puntos de Venta
    public static readonly Permission PointsOfSaleRead = new(5, "pointsofsale:read");
    public static readonly Permission PointsOfSaleWrite = new(6, "pointsofsale:write");
    
    // Permisos de Productos
    public static readonly Permission ProductsRead = new(7, "products:read");
    public static readonly Permission ProductsWrite = new(8, "products:write");
    
    // Permisos de Órdenes
    public static readonly Permission OrdersRead = new(9, "orders:read");
    public static readonly Permission OrdersWrite = new(10, "orders:write");
    
    // Permisos Administrativos
    public static readonly Permission AdminAccess = new(11, "admin:access");
    
    public int Id { get; init; }
    public string Name { get; init; }  // Formato: "resource:action"
}
```

### Role (Roles)
```csharp
// src/Conaprole.Orders.Domain/Users/Role.cs
public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role API = new(2, "API");
    public static readonly Role Distributor = new(3, "Distributor");
    public static readonly Role Administrator = new(4, "Administrator");
    
    public int Id { get; init; }
    public string Name { get; init; }
    public ICollection<User> Users { get; init; } = new List<User>();
    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}
```

#### Descripción de Roles

**Registered (Usuario Registrado)**
- Rol básico asignado a todos los usuarios al registrarse
- Permisos: Lectura básica de su propia información
- Uso: Usuarios finales del sistema

**API (Acceso Programático)**
- Rol para sistemas externos que consumen la API
- Permisos: Acceso específico según integración
- Uso: Servicios, aplicaciones externas, integraciones

**Distributor (Distribuidor)**
- Rol para usuarios distribuidores
- Permisos: Gestión de órdenes, productos, puntos de venta
- Uso: Socios comerciales, distribuidores autorizados

**Administrator (Administrador)**
- Rol con acceso completo al sistema
- Permisos: Todos los permisos disponibles
- Uso: Personal administrativo, gestión del sistema

### User (Usuario)
```csharp
// src/Conaprole.Orders.Domain/Users/User.cs
public sealed class User : Entity
{
    private readonly List<Role> _roles = new();
    
    public IReadOnlyCollection<Role> Roles => _roles.ToList();
    
    public static User Create(FirstName firstName, LastName lastName, Email email)
    {
        var user = new User(Guid.NewGuid(), firstName, lastName, email);
        user._roles.Add(Role.Registered);  // Rol por defecto
        return user;
    }
}
```

### RolePermission (Relación)
```csharp
// src/Conaprole.Orders.Domain/Users/RolePermission.cs
public sealed class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}
```

## Implementación de Autorización

### 1. HasPermissionAttribute
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/HasPermissionAttribute.cs
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(permission)
    {
    }
}
```

**Uso en Controladores:**
```csharp
// src/Conaprole.Orders.Api/Controllers/Users/UsersController.cs
[HttpGet("me")]
[HasPermission(Permissions.UsersRead)]
public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
{
    // Solo usuarios con permiso "users:read" pueden acceder
}
```

### 2. Permission Requirement
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/PermissionRequirement.cs
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
```

### 3. Permission Authorization Handler
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/PermissionAuthorizationHandler.cs
protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
{
    if (context.User.Identity is not { IsAuthenticated: true })
    {
        return; // Usuario no autenticado
    }

    using var scope = _serviceProvider.CreateScope();
    var authorizationService = scope.ServiceProvider.GetRequiredService<AuthorizationService>();

    var identityId = context.User.GetIdentityId();
    var permissions = await authorizationService.GetPermissionsForUserAsync(identityId);

    if (permissions.Contains(requirement.Permission))
    {
        context.Succeed(requirement); // ✅ Autorizado
    }
    // Si no tiene el permiso, no se llama context.Succeed() = ❌ No autorizado
}
```

### 4. Dynamic Policy Provider
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/PermissionAuthorizationPolicyProvider.cs
public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
{
    var policy = await base.GetPolicyAsync(policyName);
    
    if (policy is not null)
    {
        return policy; // Política existente
    }

    // Crear política dinámicamente
    var permissionPolicy = new AuthorizationPolicyBuilder()
        .AddRequirements(new PermissionRequirement(policyName))
        .Build();

    _authorizationOptions.AddPolicy(policyName, permissionPolicy);
    
    return permissionPolicy;
}
```

## Authorization Service

### Obtener Roles de Usuario
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/AuthorizationService.cs
public async Task<UserRolesResponse> GetRolesForUserAsync(string identityId)
{
    var roles = await _dbContext.Set<User>()
        .Where(u => u.IdentityId == identityId)
        .Select(u => new UserRolesResponse
        {
            UserId = u.Id,
            Roles = u.Roles.ToList()
        })
        .FirstAsync();

    return roles;
}
```

### Obtener Permisos de Usuario
```csharp
public async Task<HashSet<string>> GetPermissionsForUserAsync(string identityId)
{
    var permissions = await _dbContext.Set<User>()
        .Where(u => u.IdentityId == identityId)
        .SelectMany(u => u.Roles.Select(r => r.Permissions))
        .FirstAsync();

    return permissions.Select(p => p.Name).ToHashSet();
}
```

## Custom Claims Transformation

### Enriquecimiento de Claims
```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/CustomClaimsTransformation.cs
public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
{
    if (principal.Identity is not { IsAuthenticated: true } ||
        principal.HasClaim(claim => claim.Type == ClaimTypes.Role))
    {
        return principal; // Ya procesado o no autenticado
    }

    using var scope = _serviceProvider.CreateScope();
    var authorizationService = scope.ServiceProvider.GetRequiredService<AuthorizationService>();

    var identityId = principal.GetIdentityId();
    var userRoles = await authorizationService.GetRolesForUserAsync(identityId);

    var claimsIdentity = new ClaimsIdentity();
    
    // Agregar claim de UserId interno
    claimsIdentity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, userRoles.UserId.ToString()));

    // Agregar claims de roles
    foreach (var role in userRoles.Roles)
    {
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
    }

    principal.AddIdentity(claimsIdentity);
    return principal;
}
```

## Definición de Permisos y Roles

### Constantes de Permisos
```csharp
// src/Conaprole.Orders.Api/Controllers/Users/Security/Permissions.cs
internal static class Permissions
{
    // Usuarios
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    
    // Distribuidores
    public const string DistributorsRead = "distributors:read";
    public const string DistributorsWrite = "distributors:write";
    
    // Puntos de Venta
    public const string PointsOfSaleRead = "pointsofsale:read";
    public const string PointsOfSaleWrite = "pointsofsale:write";
    
    // Productos
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";
    
    // Órdenes
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    
    // Administración
    public const string AdminAccess = "admin:access";
}
```

### Constantes de Roles
```csharp
// src/Conaprole.Orders.Api/Controllers/Users/Security/Roles.cs
public static class Roles
{
    public const string Registered = "Registered";
    public const string API = "API";
    public const string Distributor = "Distributor";
    public const string Administrator = "Administrator";
}
```

## Configuración en DependencyInjection

```csharp
// src/Conaprole.Orders.Infrastructure/DependencyInjection.cs
private static void AddAuthorization(IServiceCollection services)
{
    services.AddScoped<AuthorizationService>();
    
    // Claims transformation para enriquecer con roles de DB
    services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
    
    // Handler para verificación de permisos
    services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
    
    // Provider para políticas dinámicas
    services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
}
```

## Flujo de Autorización

### Verificación de Permisos
```mermaid
sequenceDiagram
    participant Controller
    participant AuthMiddleware
    participant PolicyProvider
    participant AuthHandler
    participant AuthService
    participant Database

    Controller->>AuthMiddleware: [HasPermission("users:read")]
    AuthMiddleware->>PolicyProvider: GetPolicyAsync("users:read")
    
    alt Política no existe
        PolicyProvider->>PolicyProvider: Crear PermissionRequirement("users:read")
        PolicyProvider-->>AuthMiddleware: Nueva política
    else Política existe
        PolicyProvider-->>AuthMiddleware: Política existente
    end
    
    AuthMiddleware->>AuthHandler: HandleRequirementAsync(requirement)
    AuthHandler->>AuthService: GetPermissionsForUserAsync(identityId)
    AuthService->>Database: Query permisos por IdentityId
    Database-->>AuthService: Lista de permisos
    AuthService-->>AuthHandler: HashSet<string> permisos
    
    AuthHandler->>AuthHandler: ¿permissions.Contains("users:read")?
    
    alt Permiso encontrado
        AuthHandler->>AuthHandler: context.Succeed(requirement)
        AuthHandler-->>AuthMiddleware: ✅ Autorizado
        AuthMiddleware-->>Controller: Continuar ejecución
    else Permiso no encontrado
        AuthHandler-->>AuthMiddleware: ❌ No autorizado
        AuthMiddleware-->>Controller: 403 Forbidden
    end
```

### Transformación de Claims
```mermaid
sequenceDiagram
    participant JWT_Token
    participant ClaimsTransform
    participant AuthService
    participant Database

    JWT_Token->>ClaimsTransform: ClaimsPrincipal con IdentityId
    ClaimsTransform->>ClaimsTransform: ¿Ya tiene roles?
    
    alt No tiene roles
        ClaimsTransform->>AuthService: GetRolesForUserAsync(identityId)
        AuthService->>Database: Query roles por IdentityId
        Database-->>AuthService: User con Roles
        AuthService-->>ClaimsTransform: UserRolesResponse
        
        ClaimsTransform->>ClaimsTransform: Crear ClaimsIdentity
        ClaimsTransform->>ClaimsTransform: Agregar Claim(Sub, UserId)
        ClaimsTransform->>ClaimsTransform: Agregar Claims(Role, role.Name)
        ClaimsTransform->>ClaimsTransform: principal.AddIdentity(claimsIdentity)
    end
    
    ClaimsTransform-->>JWT_Token: ClaimsPrincipal enriquecido
```

## Ventajas del Sistema

### Granularidad
- ✅ **Permisos específicos** por funcionalidad
- ✅ **Control fino** de acceso a recursos
- ✅ **Separación clara** entre autenticación y autorización

### Flexibilidad
- ✅ **Roles dinámicos** almacenados en base de datos
- ✅ **Permisos configurables** sin cambios de código
- ✅ **Políticas generadas** automáticamente

### Escalabilidad
- ✅ **Reutilizable** entre múltiples servicios
- ✅ **Extensible** para nuevos recursos
- ✅ **Mantenible** con separación de responsabilidades

### Auditoría
- ✅ **Trazabilidad** de decisiones de autorización
- ✅ **Logging** de accesos y permisos
- ✅ **Configuración** centralizada en base de datos

## Ejemplos de Uso

### Endpoints Actualmente Protegidos

#### Información del Usuario Autenticado
```csharp
// src/Conaprole.Orders.Api/Controllers/Users/UsersController.cs
[HttpGet("me")]
[HasPermission(Permissions.UsersRead)]
public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
{
    // Solo usuarios con permiso "users:read" pueden acceder
    var query = new GetLoggedInUserQuery();
    var result = await _sender.Send(query, cancellationToken);
    return Ok(result.Value);
}
```

### Endpoints Públicos (Sin Autenticación)

#### Registro de Usuario
```csharp
[AllowAnonymous]
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
{
    // Endpoint público - no requiere autenticación
    var command = new RegisterUserCommand(request.Email, request.FirstName, request.LastName, request.Password);
    var result = await _sender.Send(command, cancellationToken);
    return result.IsSuccess ? Ok() : BadRequest(result.Error);
}
```

#### Login de Usuario
```csharp
[AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> LogIn(LogInUserRequest request, CancellationToken cancellationToken)
{
    var command = new LogInUserCommand(request.Email, request.Password);
    var result = await _sender.Send(command, cancellationToken);
    return result.IsFailure ? Unauthorized(result.Error) : Ok(result.Value);
}
```

#### Refresh Token
```csharp
[AllowAnonymous]
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
{
    var command = new RefreshTokenCommand(request.RefreshToken);
    var result = await _sender.Send(command, cancellationToken);
    return result.IsFailure ? Unauthorized(result.Error) : Ok(result.Value);
}
```

### Endpoints Preparados Para Autorización

**Nota**: El sistema tiene la infraestructura completa de autorización implementada. Los siguientes endpoints tienen los permisos definidos pero comentados, listos para ser activados:

#### Gestión de Productos
```csharp
[HttpGet("{id}")]
// [HasPermission(Permissions.ProductsRead)]  // ⚠️ Listo para activar
public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)

[HttpPost]
// [HasPermission(Permissions.ProductsWrite)]  // ⚠️ Listo para activar
public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
```

#### Gestión de Roles
```csharp
[HttpPost("{userId}/assign-role")]
// [HasPermission(Permissions.UsersWrite)]  // ⚠️ Listo para activar
public async Task<IActionResult> AssignRole(Guid userId, AssignRoleRequest request, CancellationToken cancellationToken)

[HttpPost("{userId}/remove-role")]  
// [HasPermission(Permissions.UsersWrite)]  // ⚠️ Listo para activar
public async Task<IActionResult> RemoveRole(Guid userId, RemoveRoleRequest request, CancellationToken cancellationToken)
```

### Verificación Programática de Permisos

```csharp
// En cualquier servicio o controlador
public class SomeBusinessService
{
    private readonly AuthorizationService _authorizationService;
    private readonly IUserContext _userContext;

    public async Task<Result> PerformSensitiveOperation()
    {
        // Verificar permisos programáticamente
        var permissions = await _authorizationService
            .GetPermissionsForUserAsync(_userContext.IdentityId);
        
        if (!permissions.Contains("admin:access"))
        {
            return Result.Failure(UserErrors.InsufficientPermissions);
        }

        // Continuar con la operación sensible
        return Result.Success();
    }
}
```

---

*Ver también: [Authentication](./authentication.md) | [Implementation Guide](./implementation-guide.md)*