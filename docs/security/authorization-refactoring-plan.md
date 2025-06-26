# 🔧 Plan de Acción: Refactorizaciones de Autorización y Tests

## 📋 Issues Ejecutables Propuestos

Este documento presenta los issues específicos que deben crearse para implementar las refactorizaciones identificadas en el análisis exhaustivo de autorización.

---

## 🔴 Alta Prioridad

### Issue #1: Corregir Errores de Compilación en Tests de Autorización

**Descripción**: Resolver errores de compilación reportados en `test/Conaprole.Orders.Api.FunctionalTests/Authorization/README.md`

**Tareas**:
- [ ] Agregar imports faltantes para `RegisterUserRequest` y `LogInUserRequest`
- [ ] Convertir arrays a listas en DTOs donde sea necesario  
- [ ] Corregir signatures de constructores de DTOs
- [ ] Validar que todos los tests compilen correctamente

**Archivos Afectados**:
- `test/Conaprole.Orders.Api.FunctionalTests/Authorization/*.cs`
- DTOs en `src/Conaprole.Orders.Api/Controllers/*/Dtos/`

**Criterios de Aceptación**:
- Todos los tests de autorización compilan sin errores
- No se introducen nuevos warnings de compilación
- Tests pueden ejecutarse (aunque fallen por infraestructura)

---

### Issue #2: Estandarizar Decoradores en UsersController

**Descripción**: Corregir inconsistencias en decoradores de autorización del controlador de usuarios

**Problemas Identificados**:

1. **DeleteUser con decorador mixto**:
   ```csharp
   // Actual (problemático)
   [HasPermission(Permissions.UsersWrite)]
   [Authorize(Roles = $"{Roles.Administrator},{Roles.API}")]
   
   // Propuesto (consistente)  
   [HasPermission(Permissions.AdminAccess)]
   ```

2. **ChangePassword sin permisos específicos**:
   ```csharp
   // Actual (insuficiente)
   [Authorize]
   
   // Propuesto (específico)
   [HasPermission(Permissions.UsersWrite)]
   ```

**Tareas**:
- [ ] Reemplazar decorador mixto en `DeleteUser` por `[HasPermission(Permissions.AdminAccess)]`
- [ ] Agregar `[HasPermission(Permissions.UsersWrite)]` a `ChangePassword`
- [ ] Actualizar tests correspondientes para reflejar nuevos permisos
- [ ] Documentar cambios en guía de implementación

**Archivos Afectados**:
- `src/Conaprole.Orders.Api/Controllers/Users/UsersController.cs`
- `test/Conaprole.Orders.Api.FunctionalTests/Authorization/UsersControllerAuthorizationTests.cs`

**Criterios de Aceptación**:
- Decoradores siguen patrón consistente con resto de controladores
- Tests de autorización pasan para nuevos permisos
- Documentación actualizada refleja cambios

---

### Issue #3: Reducir Dependencias de Docker en Tests de Autorización

**Descripción**: Crear alternativas de testing que no dependan de containers Docker

**Problema Actual**:
```
Docker.DotNet.DockerApiException : Docker API responded with status code=InternalServerError
```

**Solución Propuesta**:

1. **Crear tests unitarios de autorización**:
   ```csharp
   public class AuthorizationUnitTests
   {
       [Fact]
       public async Task PermissionAuthorizationHandler_UserHasPermission_ShouldSucceed()
       {
           // Test con mocks, sin Docker
       }
   }
   ```

2. **Crear helper mockeado**:
   ```csharp
   public static class MockAuthorizationHelper
   {
       public static void SetupUserWithPermission(HttpClient client, string permission)
       {
           // Setup con JWT mockeado, sin Keycloak
       }
   }
   ```

**Tareas**:
- [ ] Crear proyecto de tests unitarios para autorización
- [ ] Implementar mocks para `AuthorizationService`
- [ ] Crear helper para JWT tokens mockeados
- [ ] Migrar tests críticos a versión unitaria
- [ ] Mantener tests funcionales como opción con Docker

**Archivos a Crear**:
- `test/Conaprole.Orders.Authorization.UnitTests/`
- `test/Conaprole.Orders.Authorization.UnitTests/Helpers/MockAuthorizationHelper.cs`

**Criterios de Aceptación**:
- Tests unitarios cubren escenarios críticos de autorización
- Tests ejecutan sin dependencias externas
- Tests funcionales siguen disponibles para integration testing completo

---

## 🟡 Media Prioridad

### Issue #4: Implementar Autorización Basada en Recursos

**Descripción**: Agregar verificación de permisos a nivel de recurso específico

**Casos de Uso**:
- Usuarios solo pueden ver/editar su propio perfil (excepto admins)
- Distribuidores solo pueden ver órdenes de sus puntos de venta
- Puntos de venta solo pueden crear órdenes para sí mismos

**Implementación Propuesta**:

1. **Crear ResourceAuthorizationAttribute**:
   ```csharp
   [ResourceAuthorization(Permissions.UsersRead, ResourceType.User)]
   public async Task<IActionResult> GetUser(Guid userId)
   ```

2. **Implementar ResourceAuthorizationHandler**:
   ```csharp
   public class ResourceAuthorizationHandler : AuthorizationHandler<ResourceRequirement>
   {
       protected override async Task HandleRequirementAsync(
           AuthorizationHandlerContext context,
           ResourceRequirement requirement)
       {
           // Lógica para verificar acceso a recurso específico
       }
   }
   ```

**Tareas**:
- [ ] Diseñar interfaz `IResourceAccessValidator`
- [ ] Implementar `ResourceAuthorizationAttribute`
- [ ] Crear handlers para cada tipo de recurso
- [ ] Actualizar controladores con autorización basada en recursos
- [ ] Crear tests para nuevos escenarios de autorización

**Archivos a Crear**:
- `src/Conaprole.Orders.Infrastructure/Authorization/ResourceAuthorizationAttribute.cs`
- `src/Conaprole.Orders.Infrastructure/Authorization/IResourceAccessValidator.cs`

**Criterios de Aceptación**:
- Usuarios no pueden acceder a recursos de otros usuarios
- Distribuidores tienen acceso limitado a sus recursos
- Admins mantienen acceso completo
- Tests verifican todas las combinaciones de permisos de recursos

---

### Issue #5: Simplificar Infrastructure de Tests Funcionales

**Descripción**: Refactorizar `BaseFunctionalTest` para reducir complejidad

**Problemas Actuales**:
- Lógica compleja de sincronización Keycloak-Database
- Setup manual de usuarios cuando hay conflictos
- Dependencia fuerte en estado de containers externos

**Mejoras Propuestas**:

1. **Separar responsabilidades**:
   ```csharp
   public class KeycloakTestManager
   {
       public async Task<string> CreateTestUserAsync(string email, string password);
       public async Task<string> GetAuthTokenAsync(string email, string password);
   }
   
   public class DatabaseTestManager  
   {
       public async Task<Guid> CreateUserWithPermissionsAsync(string[] permissions);
       public async Task CleanupTestDataAsync();
   }
   ```

2. **Crear factory de usuarios de test**:
   ```csharp
   public class TestUserFactory
   {
       public static TestUser WithPermissions(params string[] permissions);
       public static TestUser WithRole(string roleName);
       public static TestUser Anonymous();
   }
   ```

**Tareas**:
- [ ] Extraer lógica de Keycloak a clase dedicada
- [ ] Extraer lógica de Database a clase dedicada  
- [ ] Crear factory pattern para usuarios de test
- [ ] Simplificar `BaseFunctionalTest`
- [ ] Actualizar todos los tests para usar nueva infrastructure

**Archivos Afectados**:
- `test/Conaprole.Orders.Api.FunctionalTests/Infrastructure/BaseFunctionalTest.cs`
- `test/Conaprole.Orders.Api.FunctionalTests/Authorization/AuthorizationTestHelper.cs`

**Criterios de Aceptación**:
- Setup de tests es más rápido y confiable
- Lógica de creación de usuarios está centralizada
- Tests son más fáciles de escribir y mantener
- Reducción en tiempo de ejecución de tests

---

## 🟢 Baja Prioridad

### Issue #6: Agregar Tests de Performance para Autorización

**Descripción**: Validar que verificación de permisos no impacte performance significativamente

**Métricas a Medir**:
- Tiempo de verificación de permisos por request
- Impacto de claims transformation
- Queries a base de datos para permisos

**Tests Propuestos**:
```csharp
[Fact]
public async Task AuthorizationCheck_Under100Milliseconds()
{
    // Test de performance para verificación de permisos
}

[Fact]  
public async Task ClaimsTransformation_CacheEffective()
{
    // Test de efectividad de cache de permisos
}
```

**Tareas**:
- [ ] Crear benchmarks para verificación de permisos
- [ ] Implementar métricas de autorización
- [ ] Agregar tests de carga para endpoints protegidos
- [ ] Documentar baseline de performance

---

### Issue #7: Documentar Patrones de Autorización

**Descripción**: Crear guía completa para desarrolladores sobre autorización

**Contenido Propuesto**:
- Cuándo usar cada tipo de permiso
- Patrones para nuevos controladores
- Mejores prácticas para tests de autorización
- Troubleshooting común

**Tareas**:
- [ ] Crear guía de patrones de autorización
- [ ] Documentar proceso de agregar nuevos permisos
- [ ] Crear examples para casos comunes
- [ ] Agregar diagramas de flujo de autorización

---

## 📈 Implementación Gradual

### Fase 1: Estabilización (Alta Prioridad)
**Objetivo**: Tests ejecutables y decoradores consistentes
- Issue #1: Corregir errores de compilación
- Issue #2: Estandarizar decoradores
- Issue #3: Reducir dependencias de Docker

### Fase 2: Mejoras (Media Prioridad)  
**Objetivo**: Autorización más robusta y tests mantenibles
- Issue #4: Autorización basada en recursos
- Issue #5: Simplificar infrastructure de tests

### Fase 3: Optimización (Baja Prioridad)
**Objetivo**: Performance y documentación
- Issue #6: Tests de performance
- Issue #7: Documentación completa

---

## 🎯 Criterios de Éxito Global

Al completar todas las refactorizaciones:

- ✅ **Compilación**: Todos los tests compilan sin errores
- ✅ **Ejecución**: Tests de autorización ejecutan de forma confiable
- ✅ **Consistencia**: Decoradores siguen patrones uniformes
- ✅ **Cobertura**: Todos los endpoints tienen tests de autorización
- ✅ **Mantenibilidad**: Infrastructure de tests es simple y extensible
- ✅ **Seguridad**: Autorización verifica permisos a nivel granular
- ✅ **Performance**: Verificación de permisos no impacta latencia
- ✅ **Documentación**: Patrones están bien documentados

---

*Este plan de acción complementa el análisis exhaustivo y proporciona una hoja de ruta clara para implementar todas las mejoras identificadas.*