# 🛡️ Documentación de Seguridad - Conaprole Orders

## Índice

1. [**Arquitectura de Seguridad**](./architecture.md) - Visión general del sistema de seguridad
2. [**Autenticación**](./authentication.md) - JWT y integración con Keycloak
3. [**Autorización**](./authorization.md) - Sistema de permisos y roles
4. [**Integración Keycloak**](./keycloak-integration.md) - Configuración y gestión de usuarios
5. [**Diagramas**](./diagrams.md) - Diagramas de flujo y arquitectura
6. [**Guía de Implementación**](./implementation-guide.md) - Como agregar nuevos permisos y roles

## Documentos por Audiencia

### 👨‍💼 **Para Managers y Arquitectos**
- [Arquitectura de Seguridad](./architecture.md) - Componentes y principios
- [Diagramas](./diagrams.md) - Visualización de la arquitectura

### 👨‍💻 **Para Desarrolladores**  
- [Autenticación](./authentication.md) - Implementación JWT/Keycloak
- [Autorización](./authorization.md) - Sistema de permisos
- [Guía de Implementación](./implementation-guide.md) - Tutoriales paso a paso

### 🔧 **Para DevOps/SysAdmins**
- [Integración Keycloak](./keycloak-integration.md) - Configuración y despliegue
- [Diagramas](./diagrams.md) - Arquitectura de despliegue

## Resumen Ejecutivo

El sistema **Conaprole Orders** implementa un esquema de seguridad robusto basado en:

### 🔐 Autenticación
- **JWT Bearer Tokens** gestionados por Keycloak
- **Refresh Token** support para renovación automática de tokens
- **Transformación de Claims** para enriquecimiento de roles
- **Múltiples clientes** Keycloak (admin y auth)
- **Validación robusta** de tokens con verificación de firma, expiración e issuer

### 🛂 Autorización  
- **Sistema basado en permisos** granulares con 11 permisos específicos
- **4 roles diferenciados**: Registered, API, Administrator, Distributor
- **Base de datos como única fuente de verdad** - sin dependencia en tokens JWT para permisos
- **Políticas de autorización** generadas dinámicamente
- **Middleware de autorización** personalizado con HasPermission attribute

### 🔗 Keycloak
- **Gestión centralizada** de usuarios e identidades
- **Separación de responsabilidades**: Keycloak para autenticación, PostgreSQL para autorización
- **Solo para identidad**: No transporta roles/permisos en tokens JWT

## Beneficios del Enfoque

- ✅ **Escalabilidad**: Fácil adición de nuevos servicios
- ✅ **Mantenibilidad**: Separación clara de responsabilidades  
- ✅ **Seguridad**: Gestión centralizada de identidades
- ✅ **Flexibilidad**: Roles y permisos configurables
- ✅ **Auditoría**: Trazabilidad completa de accesos

## Ubicación de Componentes

```
src/
├── Conaprole.Orders.Api/
│   ├── Controllers/
│   │   ├── Users/
│   │   │   ├── Security/                    # Definición de permisos y roles
│   │   │   │   ├── Permissions.cs           # 11 permisos definidos
│   │   │   │   └── Roles.cs                 # 4 roles del sistema
│   │   │   └── UsersController.cs           # Endpoints de gestión de usuarios
│   │   ├── PointsOfSale/                    # Controladores de puntos de venta
│   │   ├── Products/                        # Controladores de productos
│   │   └── Other controllers...             # Otros recursos protegidos
│   └── Program.cs                           # Configuración JWT y middleware
├── Conaprole.Orders.Application/
│   ├── Abstractions/Authentication/         # Interfaces de autenticación
│   └── Users/                               # Use cases de gestión de usuarios
├── Conaprole.Orders.Domain/Users/           # Modelos de dominio (User, Role, Permission)
└── Conaprole.Orders.Infrastructure/
    ├── Authentication/                      # Servicios de autenticación y JWT
    │   ├── JwtService.cs                    # Gestión de tokens y refresh
    │   ├── AuthenticationService.cs         # Registro de usuarios en Keycloak
    │   └── Models/                          # Modelos de Keycloak
    ├── Authorization/                       # Handlers y políticas de autorización
    │   ├── AuthorizationService.cs          # Servicios de permisos y roles
    │   ├── HasPermissionAttribute.cs        # Atributo para proteger endpoints
    │   └── PermissionAuthorizationHandler.cs # Handler de verificación
    └── DependencyInjection.cs              # Configuración de servicios
```

---

*Última actualización: Diciembre 2024*

## Permisos y Roles Implementados

### 🔑 Permisos del Sistema
El sistema cuenta con **11 permisos granulares** organizados por recursos:

**Usuarios:**
- `users:read` - Lectura de información de usuarios
- `users:write` - Creación y modificación de usuarios

**Distribuidores:**
- `distributors:read` - Consulta de distribuidores
- `distributors:write` - Gestión de distribuidores

**Puntos de Venta:**
- `pointsofsale:read` - Consulta de puntos de venta
- `pointsofsale:write` - Gestión de puntos de venta

**Productos:**
- `products:read` - Consulta de productos
- `products:write` - Gestión de productos

**Órdenes:**
- `orders:read` - Consulta de órdenes
- `orders:write` - Creación y modificación de órdenes

**Administración:**
- `admin:access` - Acceso completo administrativo

### 👥 Roles del Sistema
- **Registered** - Usuario registrado básico
- **API** - Acceso programático de sistemas externos
- **Distributor** - Distribuidor con acceso a órdenes y productos
- **Administrator** - Acceso completo al sistema