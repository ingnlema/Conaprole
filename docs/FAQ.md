# ❓ FAQ - Preguntas Frecuentes

> **Propósito**: Responder preguntas comunes sobre el proyecto Conaprole Orders API  
> **Audiencia**: Nuevos desarrolladores, equipos de onboarding, stakeholders  
> **Prerrequisitos**: Ninguno - documento de referencia rápida

## 🎯 Objetivos

Proporcionar respuestas rápidas y precisas a las preguntas más comunes sobre:

- Configuración del entorno de desarrollo
- Arquitectura y patrones utilizados
- Autorización y seguridad
- Testing y calidad de código

---

## 🏗️ Arquitectura y Desarrollo

### ¿Qué arquitectura utiliza el proyecto?

El proyecto implementa **Clean Architecture** con los siguientes patrones:

- **CQRS** con MediatR para separar comandos y consultas
- **DDD** (Domain-Driven Design) para modelado del dominio
- **Repository Pattern** para acceso a datos
- **Dependency Injection** para inversión de control

📖 **Más información**: [Clean Architecture](architecture/clean-architecture.md)

### ¿Qué tecnologías principales utiliza?

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - API Web
- **Entity Framework Core** - ORM para acceso a datos
- **PostgreSQL** - Base de datos principal
- **Keycloak** - Autenticación y gestión de identidades
- **Docker** - Containerización

### ¿Cómo está organizado el código?

```
src/
├── Conaprole.Orders.Domain/      # Entidades y reglas de negocio
├── Conaprole.Orders.Application/ # Casos de uso (CQRS)
├── Conaprole.Orders.Infrastructure/ # Acceso a datos y servicios
└── Conaprole.Orders.Api/         # Controllers y endpoints REST
```

📖 **Más información**: [Convenciones de Código](architecture/convenciones-codigo.md)

---

## 🔒 Seguridad y Autorización

### ¿Cómo funciona la autorización?

El sistema utiliza **autorización basada en permisos granulares**:

1. **Autenticación**: JWT tokens de Keycloak (solo para identidad)
2. **Autorización**: Consulta permisos en PostgreSQL en tiempo real
3. **Aplicación**: Decorador `[HasPermission]` en endpoints

```csharp
[HttpGet("{id}")]
[HasPermission(Permissions.OrdersRead)]
public async Task<IActionResult> GetOrder(Guid id)
```

📖 **Más información**: [Autorización](security/authorization.md)

### ¿Qué permisos existen?

Los permisos siguen el formato `resource:action`:

- `users:read`, `users:write`
- `orders:read`, `orders:write`
- `products:read`, `products:write`
- `distributors:read`, `distributors:write`
- `pointsofsale:read`, `pointsofsale:write`
- `admin:access`

### ¿Los permisos se pueden cambiar sin reiniciar la aplicación?

**Sí**. Los permisos se consultan en tiempo real desde la base de datos.
Cambios en permisos se aplican inmediatamente sin necesidad de reiniciar.

---

## 🧪 Testing y Calidad

### ¿Qué tipos de tests existen?

El proyecto implementa una **pirámide de tests**:

- **Tests Unitarios** (Domain + Application): ~50 tests
- **Tests de Integración** (Infrastructure): ~15 tests  
- **Tests Funcionales** (End-to-End): ~80 tests

📖 **Más información**: [Arquitectura de Pruebas](quality/arquitectura-pruebas.md)

### ¿Cómo ejecutar los tests?

```bash
# Todos los tests
dotnet test

# Solo tests unitarios
dotnet test --filter Category=Unit

# Solo tests de integración
dotnet test --filter Category=Integration

# Solo tests funcionales
dotnet test --filter Category=Functional
```

### ¿Qué herramientas de calidad se usan?

- **FluentAssertions** - Assertions expresivas
- **NSubstitute** - Mocking y stubbing
- **TestContainers** - Tests de integración con Docker
- **Coverlet** - Cobertura de código

---

## 🚀 Desarrollo Local

### ¿Cómo configurar el entorno de desarrollo?

1. **Instalar prerrequisitos**:
   - .NET 8.0 SDK
   - Docker Desktop
   - IDE (Visual Studio, VS Code, Rider)

2. **Clonar y restaurar**:

   ```bash
   git clone <repository>
   cd Conaprole
   dotnet restore
   ```

3. **Configurar base de datos**:

   ```bash
   docker-compose up -d postgres
   dotnet ef database update
   ```

📖 **Más información**: [Integration Tests Setup](testing/integration-tests-setup.md)

### ¿Cómo verificar que todo funciona?

```bash
# Compilar proyecto
dotnet build

# Ejecutar tests
dotnet test

# Validar documentación
make doc-verify
```

### ¿Qué puertos utiliza la aplicación?

- **API**: `http://localhost:5000`, `https://localhost:5001`
- **PostgreSQL**: `localhost:5432`
- **Keycloak**: `http://localhost:8080`

---

## 📚 Documentación

### ¿Cómo generar documentación de la API?

La documentación se genera automáticamente con **Swagger/OpenAPI**:

- **URL**: `https://localhost:5001/swagger`
- **JSON**: `https://localhost:5001/swagger/v1/swagger.json`

### ¿Cómo validar que la documentación está actualizada?

```bash
# Validar formato Markdown
make doc-lint

# Verificar que ejemplos de código compilan
make doc-verify

# Todas las validaciones
make doc-all
```

### ¿Cómo contribuir a la documentación?

1. **Usar plantilla**: Basar nuevos documentos en [`_TEMPLATE.md`](_TEMPLATE.md)
2. **Validar formato**: Ejecutar `markdownlint` antes de enviar
3. **Verificar ejemplos**: Asegurar que snippets de código compilan

---

## 🐛 Troubleshooting

### Error de compilación: "Nullable reference types"

**Solución**: Las advertencias de nullable references no impiden la compilación.
Para suprimirlas, usar `#nullable disable` o declarar propiedades como nullable.

### Tests fallan por Docker

**Problema**: TestContainers requiere Docker Desktop en ejecución.

**Solución**:

```bash
# Verificar Docker
docker ps

# Iniciar Docker Desktop si no está corriendo
```

### Error de conexión a base de datos

**Problema**: Connection string incorrecta o PostgreSQL no disponible.

**Solución**:

```bash
# Verificar PostgreSQL
docker-compose up -d postgres

# Aplicar migraciones
dotnet ef database update
```

### Error 403 Forbidden en endpoints

**Problema**: Usuario no tiene permisos suficientes.

**Solución**:

1. Verificar token JWT válido
2. Confirmar permisos en base de datos
3. Revisar decorador `[HasPermission]` en endpoint

---

## 📞 Soporte

### ¿Dónde reportar bugs o sugerir mejoras?

- **Issues**: Crear issue en el repositorio
- **Documentación**: PRs con mejoras son bienvenidos
- **Contacto**: @ingnlema, @FernandoMachado

### ¿Cómo obtener ayuda adicional?

1. **Revisar documentación**: [README principal](README.md)
2. **Consultar logs**: Aplicación incluye logging detallado
3. **Tests**: Revisar tests existentes como ejemplos
4. **Crear issue**: Para problemas específicos

---

> **Última actualización**: 2025-07-02  
> **Versión FAQ**: 1.0  
> **Estado**: ✅ Actualizado
