# API Documentation Guidelines - Conaprole Orders API

Esta guía establece las mejores prácticas para mantener y mejorar la documentación de la API de Conaprole usando Swagger/OpenAPI.

## Configuración Base

### XML Documentation
- **SIEMPRE** habilitar la generación de documentación XML en el proyecto:
  ```xml
  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
  ```
- El archivo XML se incluye automáticamente en la configuración de Swagger en `Program.cs`

### Swagger Configuration
- Configuración centralizada en `Program.cs` con información básica de la API
- Uso de `SwaggerExamplesFromAssemblyOf` para cargar ejemplos automáticamente
- Configuración de seguridad JWT para endpoints protegidos

## Documentación de Controladores

### Estructura Requerida
```csharp
/// <summary>
/// Breve descripción del propósito del controlador
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "NombreGrupo")]
public class MiController : ControllerBase
{
    /// <summary>
    /// Descripción detallada del constructor
    /// </summary>
    /// <param name="sender">Descripción del parámetro</param>
    public MiController(ISender sender) { }
}
```

### Documentación de Métodos
```csharp
/// <summary>
/// Descripción completa de qué hace el endpoint
/// </summary>
/// <param name="parametro">Descripción del parámetro</param>
/// <param name="cancellationToken">Token de cancelación</param>
/// <returns>Descripción del valor retornado</returns>
/// <response code="200">Descripción del caso exitoso</response>
/// <response code="400">Descripción del caso de error</response>
/// <response code="404">Descripción del caso no encontrado</response>
[SwaggerOperation(
    Summary = "Resumen corto", 
    Description = "Descripción más detallada con ejemplos si es necesario")]
[HttpGet("{id}")]
[ProducesResponseType(typeof(MiResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
[SwaggerRequestExample(typeof(MiRequest), typeof(MiRequestExample))]
public async Task<IActionResult> MiMetodo(Guid id, CancellationToken cancellationToken)
```

## Documentación de DTOs

### Request/Response Models
```csharp
/// <summary>
/// Descripción del propósito del modelo
/// </summary>
/// <param name="Campo1">Descripción detallada del campo</param>
/// <param name="Campo2">Descripción detallada del campo</param>
public record MiRequest(
    string Campo1,
    int Campo2);

// Para propiedades complejas:
/// <summary>
/// Descripción del modelo
/// </summary>
public record MiRequestComplejo
{
    /// <summary>
    /// Descripción detallada del campo con restricciones si aplica
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MiPropiedad { get; init; }
}
```

## Ejemplos (Examples)

### Implementación de Ejemplos
- Crear una clase por cada request/response que necesite ejemplo
- Ubicar en carpeta `Examples` dentro del namespace de DTOs
- Implementar `IExamplesProvider<T>`

```csharp
/// <summary>
/// Ejemplo para MiRequest
/// </summary>
public class MiRequestExample : IExamplesProvider<MiRequest>
{
    public MiRequest GetExamples()
    {
        return new MiRequest(
            Campo1: "valor-ejemplo",
            Campo2: 123
        );
    }
}
```

### Uso en Controladores
```csharp
[SwaggerRequestExample(typeof(MiRequest), typeof(MiRequestExample))]
[SwaggerResponseExample(StatusCodes.Status200OK, typeof(MiResponseExample))]
```

## Organización de APIs

### Agrupación con Tags
- Usar `ApiExplorerSettings(GroupName = "NombreGrupo")` en controladores
- Grupos recomendados:
  - "Orders" - Gestión de órdenes
  - "Products" - Catálogo de productos  
  - "Distributors" - Gestión de distribuidores
  - "Points of Sale" - Puntos de venta
  - "Users" - Gestión de usuarios

### Convenciones de Naming
- Rutas en inglés: `/api/Orders`, `/api/Products`
- Summaries en español para mejor comprensión del equipo
- Descriptions más detalladas con ejemplos cuando sea necesario

## Validación y Testing

### Tests Obligatorios
- Validar que el endpoint `/swagger/v1/swagger.json` retorna JSON válido
- Verificar información básica de la API (título, versión, descripción)
- Confirmar que existe organización por tags
- Validar accesibilidad de Swagger UI

### Ejemplo de Test
```csharp
[Fact]
public async Task SwaggerEndpoint_ShouldReturnValidOpenApiDocument()
{
    var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");
    response.EnsureSuccessStatusCode();
    
    var content = await response.Content.ReadAsStringAsync();
    var jsonDocument = JsonDocument.Parse(content);
    Assert.NotNull(jsonDocument);
}
```

## Pipeline de CI/CD

### Validación Automática
- Ejecutar tests de Swagger en el pipeline
- Validar que el build incluye documentación XML
- Verificar que no hay warnings críticos de documentación faltante

### Script de Validación (Futuro)
```bash
# Ejecutar tests de documentación
dotnet test --filter "SwaggerValidationTests" --verbosity normal

# Verificar build con documentación
dotnet build --configuration Release --verbosity normal
```

## Mejores Prácticas

### DO's ✅
- Documentar TODOS los endpoints públicos
- Incluir ejemplos para requests complejos
- Usar tipos de retorno específicos en `ProducesResponseType`
- Mantener coherencia en naming y estructura
- Actualizar documentación cuando cambie la API

### DON'Ts ❌
- No dejar endpoints sin documentación
- No usar descripciones genéricas como "Obtiene datos"
- No olvidar actualizar ejemplos cuando cambien los modelos
- No mezclar idiomas en la documentación
- No incluir información sensible en ejemplos

## Herramientas Recomendadas

### Extensiones VS Code/Visual Studio
- REST Client para probar endpoints
- Swagger Viewer para visualizar OpenAPI
- XML Documentation Comments para autocompletado

### Validación Externa
- Swagger Editor (https://editor.swagger.io/) para validar OpenAPI
- Postman para importar y probar colecciones

## Mantenimiento

### Revisión Regular
- Revisar documentación al hacer code reviews
- Actualizar ejemplos cuando cambien modelos
- Validar que nuevos endpoints tengan documentación completa
- Ejecutar tests de Swagger regularmente

### Métricas de Calidad
- Porcentaje de endpoints documentados
- Cobertura de ejemplos en requests principales
- Tiempo de response de `/swagger/v1/swagger.json`
- Ausencia de warnings de documentación XML

---

## Contacto y Soporte

Para preguntas sobre esta guía o sugerencias de mejora, contactar al equipo de desarrollo de Conaprole.

**Última actualización:** 2025-01-03
**Versión:** 1.0