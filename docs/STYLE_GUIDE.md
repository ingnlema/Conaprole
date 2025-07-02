# 📝 Guía de Estilo de Documentación

> **Propósito**: Definir reglas de formato y escritura para documentación consistente  
> **Audiencia**: Todos los contribuidores a la documentación  
> **Prerrequisitos**: Conocimientos básicos de Markdown

## 🎯 Objetivos

Establecer reglas claras para mantener documentación:

- **Consistente** - Formato y estilo uniformes
- **Legible** - Fácil de leer y navegar
- **Mantenible** - Fácil de actualizar y validar

---

## 📏 Reglas de Formato

### Markdown

- **Longitud de línea**: Máximo 120 caracteres
- **Encabezados**: Espacio antes y después de títulos
- **Listas**: Espacio antes y después de listas
- **Bloques de código**: Espacio antes y después de ```
- **Final de archivo**: Una sola línea nueva al final

### Estructura de Documentos

```markdown
# 📄 Título Principal (con emoji descriptivo)

> **Propósito**: [Descripción clara]  
> **Audiencia**: [Target específico]  
> **Prerrequisitos**: [Conocimiento requerido]

## 🎯 Objetivos

[Explicación del problema que resuelve]

---

## 📋 Contenido Principal

[Desarrollo del contenido...]

---

> **Última verificación**: YYYY-MM-DD  
> **Commit SHA**: [SHA de último commit verificado]  
> **Estado**: ✅ Verificado | ⚠️ Requiere actualización
```

## 🔧 Herramientas de Validación

### Markdownlint

```bash
# Validar formato
markdownlint-cli2 "docs/**/*.md"

# Auto-fix cuando sea posible
markdownlint-cli2 "docs/**/*.md" --fix
```

### Verificación de Código

```bash
# Verificar que snippets compilan
./scripts/verify-docs.sh

# Validación completa
make doc-all
```

## 📋 Convenciones Específicas

### Idioma

- **Documentación**: Español (para coherencia con el equipo)
- **Código**: Inglés (siguiendo estándares internacionales)
- **Comentarios en código**: Español en ejemplos de documentación

### Emojis

Usar emojis consistentes para categorías:

- 📚 Documentación general
- 🏗️ Arquitectura
- 🔒 Seguridad
- 🧪 Testing
- ⚡ Performance
- 🔧 Configuración
- ❓ FAQ/Ayuda

### Enlaces

- **Internos**: Usar rutas relativas
- **Código**: Incluir ruta del archivo fuente
- **Cross-references**: Mantener actualizados

### Ejemplos de Código

```csharp
// ✅ Bueno: Comentario en español, código en inglés
// Controlador para gestión de pedidos
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // Referencia: src/Conaprole.Orders.Api/Controllers/Orders/OrdersController.cs
}
```

## 🎯 Prioridades de Aplicación

### 📄 Documentos Críticos (Aplicar primero)

1. `docs/README.md` - Índice principal
2. `docs/architecture/clean-architecture.md` - Base arquitectónica
3. `docs/security/authorization.md` - Autorización actual
4. `docs/FAQ.md` - Preguntas frecuentes

### 📂 Categorías por Impacto

- **Alta prioridad**: Architecture overview, Security core
- **Media prioridad**: Testing guides, API documentation
- **Baja prioridad**: Detailed technical specs

---

> **Última verificación**: 2025-07-02  
> **Commit SHA**: 20c7d61  
> **Estado**: ✅ Verificado
