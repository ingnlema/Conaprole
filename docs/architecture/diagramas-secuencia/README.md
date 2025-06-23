# 🔄 Diagramas de Secuencia - Flujos Habituales

Este directorio contiene diagramas de secuencia que representan los flujos típicos y alternativos del sistema **Conaprole Orders API**, mostrando cómo se procesan las requests desde su recepción hasta la ejecución del caso de uso.

## 📋 Diagramas Disponibles

### ✅ Flujos Exitosos

#### 🔄 [Flujo Típico de Command (Escritura)](flujo-command-typical.md)
Muestra el procesamiento completo de un comando exitoso:
- Recepción de HTTP Request
- Pipeline de MediatR con Behaviors
- Validación con FluentValidation
- Ejecución del Command Handler
- Persistencia transaccional
- Respuesta 201 Created

**Casos representados:** CreateOrder, UpdateOrderStatus, AddOrderLine

#### 🔍 [Flujo Típico de Query (Lectura)](flujo-query-typical.md)
Muestra el procesamiento optimizado de una query exitosa:
- Autenticación JWT
- Pipeline simplificado para lecturas
- Query Handler con acceso directo a datos
- Proyección a DTOs de respuesta
- Respuesta 200 OK

**Casos representados:** GetOrder, GetOrders, GetDistributors

### ⚠️ Flujos Alternativos (Errores)

#### ❌ [Flujo de Command con Error de Validación](flujo-command-error-validacion.md)
Muestra el manejo de errores de validación:
- ValidationBehavior intercepta datos inválidos
- FluentValidation ejecuta reglas de negocio
- ValidationException con detalles estructurados
- ExceptionHandlingMiddleware maneja la respuesta
- Respuesta 400 Bad Request con errores detallados

**Errores representados:** Campos requeridos, formatos inválidos, reglas de negocio

#### 🔒 [Flujo de Query con Error de Autorización](flujo-query-error-auth.md)
Muestra el manejo de errores de autorización:
- JWT válido pero permisos insuficientes
- Claims Transformation y enriquecimiento
- Authorization Handler verifica permisos
- Terminación temprana del pipeline
- Respuesta 403 Forbidden

**Escenarios representados:** Usuario sin permisos, acceso a recursos restringidos

## 🏗️ Arquitectura Representada

Los diagramas reflejan la implementación de:

### **Clean Architecture**
- Separación clara de capas y responsabilidades
- Inversión de dependencias (DIP)
- Independencia de frameworks y base de datos

### **CQRS con MediatR**
- Separación de Commands (escritura) y Queries (lectura)
- Pipeline de Behaviors para cross-cutting concerns
- Handlers especializados para cada caso de uso

### **Patterns de Seguridad**
- JWT Authentication con Keycloak
- Permission-Based Authorization
- Claims Transformation y enriquecimiento

### **Manejo de Errores**
- Exception Handling Middleware centralizado
- Validation con FluentValidation
- Problem Details (RFC 7807) para respuestas de error

## 🎯 Niveles de Abstracción

Los diagramas muestran el flujo a nivel **arquitectónico**, incluyendo:

- **Componentes principales** (Controllers, MediatR, Handlers)
- **Capas de la aplicación** (API, Application, Infrastructure)
- **Cross-cutting concerns** (Logging, Validation, Authorization)
- **Puntos de decisión** y bifurcaciones del flujo
- **Interacciones con sistemas externos** (Keycloak, Database)

## 📊 Leyenda de Elementos

### Participantes
- **Cliente**: Aplicación consumidora de la API
- **API Controller**: Punto de entrada HTTP
- **Middleware**: Components transversales (Auth, Exception)
- **MediatR**: Mediator pattern para CQRS
- **Behaviors**: Pipeline cross-cutting (Validation, Logging)
- **Handlers**: Lógica específica de Command/Query
- **Repositories**: Abstracciones de acceso a datos
- **Database**: Capa de persistencia

### Estilos Visuales
- 🟢 **Verde**: Flujos exitosos y operaciones completadas
- 🔴 **Rojo**: Errores y excepciones
- 🟡 **Amarillo**: Procesos de validación y transformación
- 🔵 **Azul**: Componentes de infraestructura y datos
- ⚪ **Gris**: Middleware y componentes transversales

## 🔄 Mantenimiento

Los diagramas deben actualizarse cuando:
- Se modifiquen los pipelines de MediatR
- Se agreguen nuevos Behaviors o Middleware
- Cambien los patrones de autorización
- Se introduzcan nuevos tipos de errores o excepciones
- Se modifique la estructura de capas de la aplicación

## 📚 Referencias

- [CQRS y MediatR](../cqrs-mediator.md) - Detalles de implementación
- [Clean Architecture](../clean-architecture.md) - Principios arquitectónicos
- [Security Architecture](../security-architecture.md) - Patrones de seguridad
- [Diagramas de Seguridad](../../security/diagrams.md) - Diagramas complementarios

---

*Generado para documentar los flujos habituales de la API Core Conaprole Orders*