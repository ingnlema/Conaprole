# 🛡️ Documentación de Seguridad - Conaprole Orders

## Índice

1. [**Arquitectura de Seguridad**](./architecture.md) - Visión general del sistema de seguridad
2. [**Autenticación**](./authentication.md) - JWT y integración con Keycloak
3. [**Autorización**](./authorization.md) - Sistema de permisos y roles
4. [**Integración Keycloak**](./keycloak-integration.md) - Configuración y gestión de usuarios
5. [**Guía de Implementación**](./implementation-guide.md) - Como agregar nuevos permisos y roles
6. [**Diagramas**](./diagrams.md) - Diagramas de flujo y arquitectura

## Resumen Ejecutivo

El sistema **Conaprole Orders** implementa un esquema de seguridad robusto basado en:

### 🔐 Autenticación
- **JWT Bearer Tokens** gestionados por Keycloak
- **Transformación de Claims** para enriquecimiento de roles
- **Múltiples clientes** Keycloak (admin y auth)

### 🛂 Autorización  
- **Sistema basado en permisos** granulares
- **Roles dinámicos** almacenados en base de datos
- **Políticas de autorización** generadas dinámicamente

### 🔗 Keycloak
- **Gestión centralizada** de usuarios e identidades
- **Integración completa** con la API
- **Separación de responsabilidades** entre administración y autenticación

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
│   ├── Controllers/Users/Security/          # Definición de permisos y roles
│   └── Program.cs                           # Configuración JWT y middleware
├── Conaprole.Orders.Application/
│   └── Abstractions/Authentication/         # Interfaces de autenticación
├── Conaprole.Orders.Domain/Users/           # Modelos de dominio (User, Role, Permission)
└── Conaprole.Orders.Infrastructure/
    ├── Authentication/                      # Servicios de autenticación y JWT
    ├── Authorization/                       # Handlers y políticas de autorización
    └── DependencyInjection.cs              # Configuración de servicios
```

---

*Última actualización: Diciembre 2024*