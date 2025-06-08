# 🛠️ Guía de Implementación

Esta guía proporciona instrucciones paso a paso para desarrolladores que necesiten **agregar nuevos permisos, roles o funcionalidades de seguridad** al sistema Conaprole Orders.

## Agregar un Nuevo Permiso

### 1. Definir la Constante del Permiso

```csharp
// src/Conaprole.Orders.Api/Controllers/Users/Security/Permissions.cs
internal static class Permissions
{
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";           // ✨ NUEVO
    public const string OrdersRead = "orders:read";           // ✨ NUEVO  
    public const string OrdersWrite = "orders:write";         // ✨ NUEVO
    public const string AdminAccess = "admin:access";         // ✨ NUEVO
}
```

### 2. Crear la Entidad de Permiso en el Dominio

```csharp
// src/Conaprole.Orders.Domain/Users/Permission.cs
public sealed class Permission
{
    public static readonly Permission UsersRead = new(1, "users:read");
    public static readonly Permission UsersWrite = new(2, "users:write");     // ✨ NUEVO
    public static readonly Permission OrdersRead = new(3, "orders:read");     // ✨ NUEVO
    public static readonly Permission OrdersWrite = new(4, "orders:write");   // ✨ NUEVO
    public static readonly Permission AdminAccess = new(5, "admin:access");   // ✨ NUEVO

    private Permission(int id, string name) { Id = id; Name = name; }
    
    public int Id { get; init; }
    public string Name { get; init; }
}
```

### 3. Crear Migración de Base de Datos

```bash
# Generar migración
dotnet ef migrations add AddNewPermissions --project src/Conaprole.Orders.Infrastructure --startup-project src/Conaprole.Orders.Api
```

```csharp
// Archivo de migración generado: YYYYMMDD_AddNewPermissions.cs
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Permissions",
        columns: new[] { "Id", "Name" },
        values: new object[,]
        {
            { 2, "users:write" },
            { 3, "orders:read" },
            { 4, "orders:write" },
            { 5, "admin:access" }
        });
}
```

### 4. Aplicar el Permiso en un Controlador

```csharp
// src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.OrdersRead)]  // ✨ NUEVO PERMISO
    public async Task<IActionResult> GetOrders()
    {
        // Solo usuarios con permiso "orders:read" pueden acceder
        return Ok();
    }

    [HttpPost]
    [HasPermission(Permissions.OrdersWrite)]  // ✨ NUEVO PERMISO
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Solo usuarios con permiso "orders:write" pueden acceder
        return Ok();
    }
}
```

## Agregar un Nuevo Rol

### 1. Definir la Constante del Rol

```csharp
// src/Conaprole.Orders.Api/Controllers/Users/Security/Roles.cs
public static class Roles
{
    public const string Registered = "Registered";
    public const string Distributor = "Distributor";           // ✨ NUEVO
    public const string Manager = "Manager";                   // ✨ NUEVO
    public const string Administrator = "Administrator";       // ✨ NUEVO
}
```

### 2. Crear la Entidad de Rol en el Dominio

```csharp
// src/Conaprole.Orders.Domain/Users/Role.cs
public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role Distributor = new(2, "Distributor");         // ✨ NUEVO
    public static readonly Role Manager = new(3, "Manager");                 // ✨ NUEVO
    public static readonly Role Administrator = new(4, "Administrator");     // ✨ NUEVO

    private Role(int id, string name) { Id = id; Name = name; }
    
    public int Id { get; init; }
    public string Name { get; init; }
    public ICollection<User> Users { get; init; } = new List<User>();
    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}
```

### 3. Configurar Permisos del Rol

```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/RoleConfiguration.cs
public void Configure(EntityTypeBuilder<Role> builder)
{
    builder.HasData(
        new { Id = 1, Name = "Registered" },
        new { Id = 2, Name = "Distributor" },    // ✨ NUEVO
        new { Id = 3, Name = "Manager" },        // ✨ NUEVO
        new { Id = 4, Name = "Administrator" }   // ✨ NUEVO
    );
}
```

```csharp
// src/Conaprole.Orders.Infrastructure/Configuration/RolePermissionConfiguration.cs
public void Configure(EntityTypeBuilder<RolePermission> builder)
{
    builder.HasData(
        // Registered role
        new RolePermission { RoleId = 1, PermissionId = 1 }, // users:read

        // Distributor role
        new RolePermission { RoleId = 2, PermissionId = 1 }, // users:read
        new RolePermission { RoleId = 2, PermissionId = 3 }, // orders:read
        new RolePermission { RoleId = 2, PermissionId = 4 }, // orders:write

        // Manager role  
        new RolePermission { RoleId = 3, PermissionId = 1 }, // users:read
        new RolePermission { RoleId = 3, PermissionId = 2 }, // users:write
        new RolePermission { RoleId = 3, PermissionId = 3 }, // orders:read
        new RolePermission { RoleId = 3, PermissionId = 4 }, // orders:write

        // Administrator role (todos los permisos)
        new RolePermission { RoleId = 4, PermissionId = 1 }, // users:read
        new RolePermission { RoleId = 4, PermissionId = 2 }, // users:write
        new RolePermission { RoleId = 4, PermissionId = 3 }, // orders:read
        new RolePermission { RoleId = 4, PermissionId = 4 }, // orders:write
        new RolePermission { RoleId = 4, PermissionId = 5 }  // admin:access
    );
}
```

### 4. Crear Métodos de Asignación de Roles

```csharp
// src/Conaprole.Orders.Domain/Users/User.cs
public void AssignRole(Role role)
{
    if (_roles.Any(r => r.Id == role.Id))
    {
        return; // Ya tiene el rol
    }
    
    _roles.Add(role);
    RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, role.Id));
}

public void RemoveRole(Role role)
{
    var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
    if (existingRole != null)
    {
        _roles.Remove(existingRole);
        RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, role.Id));
    }
}
```

## Agregar Verificación Programática de Permisos

### En un Use Case (Application Layer)

```csharp
// src/Conaprole.Orders.Application/Orders/CreateOrder/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IUserContext _userContext;
    private readonly AuthorizationService _authorizationService;

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Verificar permiso programáticamente
        var permissions = await _authorizationService
            .GetPermissionsForUserAsync(_userContext.IdentityId);
        
        if (!permissions.Contains("orders:write"))
        {
            return Result.Failure<Guid>(UserErrors.InsufficientPermissions);
        }

        // Lógica del caso de uso
        var order = Order.Create(/* parámetros */);
        
        return order.Id;
    }
}
```

### En un Controlador

```csharp
// src/Conaprole.Orders.Api/Controllers/Users/UsersController.cs
[HttpPost("{userId}/assign-role")]
[HasPermission(Permissions.UsersWrite)]
public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleRequest request)
{
    // Verificación adicional para operaciones sensibles
    var userPermissions = await _authorizationService
        .GetPermissionsForUserAsync(_userContext.IdentityId);
    
    if (request.RoleName == "Administrator" && !userPermissions.Contains("admin:access"))
    {
        return Forbid("Solo administradores pueden asignar el rol de Administrator");
    }

    // Lógica de asignación
    var command = new AssignRoleCommand(userId, request.RoleName);
    var result = await _sender.Send(command);
    
    return result.IsSuccess ? Ok() : BadRequest(result.Error);
}
```

## Configurar Autorización por Roles

### Usando [Authorize] estándar con roles

```csharp
// Para casos simples donde solo necesitas verificar roles
[HttpGet("admin/dashboard")]
[Authorize(Roles = "Administrator,Manager")]
public async Task<IActionResult> GetAdminDashboard()
{
    // Solo administradores y managers pueden acceder
    return Ok();
}
```

### Combinando Roles y Permisos

```csharp
[HttpDelete("{userId}")]
[HasPermission(Permissions.UsersWrite)]
[Authorize(Roles = "Administrator")] // Doble verificación
public async Task<IActionResult> DeleteUser(Guid userId)
{
    // Debe tener el permiso Y ser administrador
    return Ok();
}
```

## Agregar Nuevas Políticas de Autorización

### 1. Definir Política Personalizada

```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/Policies/
public class DistributorResourceRequirement : IAuthorizationRequirement
{
    public DistributorResourceRequirement(string resource)
    {
        Resource = resource;
    }

    public string Resource { get; }
}
```

### 2. Implementar Handler de Política

```csharp
public class DistributorResourceHandler : AuthorizationHandler<DistributorResourceRequirement>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DistributorResourceRequirement requirement)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        
        if (user?.DistributorId != null)
        {
            // Verificar que el recurso pertenece al distribuidor del usuario
            if (await ResourceBelongsToDistributor(requirement.Resource, user.DistributorId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }
}
```

### 3. Registrar en DI

```csharp
// src/Conaprole.Orders.Infrastructure/DependencyInjection.cs
private static void AddAuthorization(IServiceCollection services)
{
    services.AddScoped<AuthorizationService>();
    services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
    services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services.AddTransient<IAuthorizationHandler, DistributorResourceHandler>(); // ✨ NUEVO
    services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
}
```

## Testing de Seguridad

### Unit Tests para Autorización

```csharp
// test/Conaprole.Orders.Application.UnitTests/Authorization/
public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirement_UserHasPermission_ShouldSucceed()
    {
        // Arrange
        var user = User.Create(FirstName.Create("Test"), LastName.Create("User"), Email.Create("test@test.com"));
        user.AssignRole(Role.Administrator);
        
        var requirement = new PermissionRequirement("admin:access");
        var context = new AuthorizationHandlerContext(new[] { requirement }, CreateClaimsPrincipal(user.IdentityId), null);
        
        var authService = new Mock<AuthorizationService>();
        authService.Setup(x => x.GetPermissionsForUserAsync(user.IdentityId))
                   .ReturnsAsync(new HashSet<string> { "admin:access" });

        var handler = new PermissionAuthorizationHandler(CreateServiceProvider(authService.Object));

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }
}
```

### Integration Tests para Endpoints

```csharp
// test/Conaprole.Orders.Api.FunctionalTests/Authorization/
public class UsersControllerAuthorizationTests : BaseFunctionalTest
{
    [Fact]
    public async Task GetUsers_WithoutPermission_ShouldReturn403()
    {
        // Arrange
        var token = await GetTokenForUser("user@test.com", "password");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithPermission_ShouldReturn200()
    {
        // Arrange
        var user = await CreateUserWithPermission("users:read");
        var token = await GetTokenForUser(user.Email, "password");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Configuración de Nuevos Entornos

### Staging Environment

```json
// appsettings.Staging.json
{
  "Authentication": {
    "Audience": "account",
    "ValidIssuer": "https://keycloak-staging.conaprole.com/realms/Conaprole",
    "MetadataUrl": "https://keycloak-staging.conaprole.com/realms/Conaprole/.well-known/openid-configuration",
    "RequireHttpsMetadata": true
  },
  "Keycloak": {
    "AdminUrl": "https://keycloak-staging.conaprole.com/admin/realms/Conaprole/",
    "TokenUrl": "https://keycloak-staging.conaprole.com/realms/Conaprole/protocol/openid-connect/token",
    "AdminClientId": "conaprole-admin-client",
    "AdminClientSecret": "${KEYCLOAK_ADMIN_SECRET}",
    "AuthClientId": "conaprole-auth-client",
    "AuthClientSecret": "${KEYCLOAK_AUTH_SECRET}"
  }
}
```

## Mejores Prácticas

### Naming Conventions

#### Permisos
```csharp
// Formato: "{resource}:{action}"
public const string UsersRead = "users:read";
public const string UsersWrite = "users:write";
public const string UsersDelete = "users:delete";
public const string OrdersManage = "orders:manage";
public const string ReportsView = "reports:view";
```

#### Roles
```csharp
// Usar nombres descriptivos del negocio
public const string Registered = "Registered";        // Usuario básico
public const string Distributor = "Distributor";      // Distribuidor
public const string Manager = "Manager";              // Gerente
public const string Administrator = "Administrator";  // Admin completo
```

### Principio de Menor Privilegio

```csharp
// ❌ MAL: Dar permisos amplios
public static Role BasicUser = new(1, "BasicUser");
// Permisos: users:*, orders:*, reports:*, admin:*

// ✅ BIEN: Dar permisos específicos
public static Role RegisteredUser = new(1, "RegisteredUser");
// Permisos: users:read, orders:own:read
```

### Configuración de Claims

```csharp
// src/Conaprole.Orders.Infrastructure/Authorization/CustomClaimsTransformation.cs
public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
{
    // ✅ Verificar si ya fue procesado para evitar procesamiento duplicado
    if (principal.HasClaim(claim => claim.Type == ClaimTypes.Role))
    {
        return principal;
    }

    // ✅ Usar scoped services para evitar problemas de concurrencia
    using var scope = _serviceProvider.CreateScope();
    var authService = scope.ServiceProvider.GetRequiredService<AuthorizationService>();

    // Resto de la lógica...
}
```

### Manejo de Errores

```csharp
// src/Conaprole.Orders.Api/Middleware/ExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (UnauthorizedAccessException)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("No autorizado");
    }
    catch (ForbiddenAccessException ex)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync($"Acceso denegado: {ex.Message}");
    }
}
```

## Checklist de Implementación

### ✅ Agregar Nuevo Permiso
- [ ] Definir constante en `Permissions.cs`
- [ ] Crear entidad en `Permission.cs`
- [ ] Generar y aplicar migración
- [ ] Asignar permiso a roles apropiados
- [ ] Aplicar `[HasPermission]` en controladores
- [ ] Crear tests unitarios e integración
- [ ] Actualizar documentación

### ✅ Agregar Nuevo Rol
- [ ] Definir constante en `Roles.cs`
- [ ] Crear entidad en `Role.cs`
- [ ] Configurar relaciones rol-permisos
- [ ] Generar y aplicar migración
- [ ] Crear métodos de asignación en `User.cs`
- [ ] Implementar use cases de gestión de roles
- [ ] Crear tests unitarios e integración
- [ ] Actualizar documentación

### ✅ Verificar Configuración
- [ ] URLs de Keycloak correctas por ambiente
- [ ] Secretos de clientes configurados
- [ ] HTTPS habilitado en producción
- [ ] Logging de seguridad configurado
- [ ] Tests de seguridad pasando
- [ ] Documentación actualizada

---

*Ver también: [Authorization](./authorization.md) | [Keycloak Integration](./keycloak-integration.md) | [Diagrams](./diagrams.md)*