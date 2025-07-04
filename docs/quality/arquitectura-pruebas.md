# 🏗️ Arquitectura de Pruebas y Aseguramiento de la Calidad

## Introducción

La arquitectura de pruebas del proyecto Conaprole API Core representa una implementación exhaustiva de **estrategias de aseguramiento de la calidad** que abraza principios modernos de ingeniería de software. Esta documentación expone las decisiones arquitectónicas, fundamentos teóricos y beneficios estratégicos que guían la implementación del sistema de testing del proyecto.

## 1. Estrategia de Pruebas

### 1.1 Principios Fundamentales

La estrategia de testing se fundamenta en los siguientes principios arquitectónicos:

#### **Principio de Separación de Responsabilidades**

Cada tipo de prueba tiene una responsabilidad específica y bien delimitada, evitando solapamientos y garantizando cobertura completa sin redundancia innecesaria.

#### **Principio de la Pirámide de Testing**

Se adopta el modelo piramidal donde las pruebas unitarias forman la base sólida (mayor cantidad, ejecución rápida), seguidas por pruebas de integración y finalmente pruebas funcionales en la cúspide (menor cantidad, mayor cobertura de escenarios).

#### **Principio de Feedback Rápido**

La arquitectura prioriza tiempos de ejecución optimizados para proporcionar retroalimentación inmediata durante el desarrollo, facilitando la detección temprana de defectos.

#### **Principio de Aislamiento**

Cada prueba es independiente y no depende del estado o resultado de otras pruebas, garantizando la repetibilidad y confiabilidad de los resultados.

### 1.2 Objetivos Estratégicos

#### **Cobertura Integral por Capas**

- **Dominio**: Validación de lógica de negocio e invariantes
- **Aplicación**: Verificación de casos de uso y comportamientos transversales
- **Integración**: Validación de interacciones entre componentes
- **API**: Verificación de contratos de interfaz externa

#### **Calidad del Código**

- Garantizar mantenibilidad del código de producción
- Documentar comportamientos esperados mediante especificaciones ejecutables
- Facilitar refactorización segura con red de seguridad robusta

#### **Confiabilidad del Sistema**

- Detectar regresiones antes del despliegue
- Validar requisitos funcionales y no funcionales
- Asegurar estabilidad en diferentes entornos

### 1.3 Tipos de Cobertura

#### **Cobertura Funcional**

Validación de que el sistema cumple con los requisitos funcionales especificados, incluyendo casos de uso principales y alternativos.

#### **Cobertura de Integración**

Verificación de la correcta interacción entre componentes internos y servicios externos, incluyendo persistencia, autenticación y comunicaciones.

#### **Cobertura de Comportamiento**

Validación de comportamientos transversales como validación, logging, manejo de errores y autorización.

#### **Cobertura de Contratos**

Verificación de que las interfaces públicas (APIs REST) mantienen sus contratos y no introducen cambios incompatibles.

## 2. Tipos de Pruebas Aplicadas

### 2.1 Pruebas Unitarias de Dominio

#### **Propósito Conceptual**

Las pruebas unitarias de dominio se enfocan en validar la **pureza de la lógica de negocio**, asegurando que las reglas de dominio, invariantes y comportamientos específicos del negocio se mantengan íntegros independientemente de la infraestructura.

#### **Áreas de Aplicación**

- Entidades de dominio (Order, Product, Distributor, etc.)
- Value Objects (Email, Money, Address, etc.)
- Agregados y sus invariantes
- Domain Services especializados
- Especificaciones de negocio

#### **Beneficios Estratégicos**

- **Documentación Viva**: Las pruebas actúan como especificación ejecutable de las reglas de negocio
- **Evolución Segura**: Permiten modificar implementaciones manteniendo la semántica de negocio
- **Detección Temprana**: Identifican violaciones de reglas de negocio en el momento de desarrollo

### 2.2 Pruebas Unitarias de Aplicación

#### **Propósito Conceptual**

Validan la **orquestación de casos de uso** y comportamientos transversales, asegurando que la capa de aplicación coordine correctamente los componentes de dominio e infraestructura.

#### **Áreas de Aplicación**

- Command Handlers (CQRS pattern)
- Query Handlers (CQRS pattern)
- Pipeline Behaviors (validación, logging, autorización)
- Application Services
- Mappers y transformaciones

#### **Beneficios Estratégicos**

- **Validación de Flujos**: Aseguran que los casos de uso ejecuten la secuencia correcta de operaciones
- **Comportamientos Transversales**: Validan aspectos como validación, auditoría y manejo de errores
- **Aislamiento de Dependencias**: Verifican interacciones sin depender de implementaciones concretas

### 2.3 Pruebas de Integración

#### **Propósito Conceptual**

Verifican la **colaboración real entre componentes**, validando que la integración entre capas y servicios externos funcione correctamente en un entorno controlado pero realista.

#### **Áreas de Aplicación**

- Persistencia de datos (Entity Framework, PostgreSQL)
- Casos de uso completos end-to-end
- Configuración de inyección de dependencias
- Mapeo objeto-relacional
- Transacciones y consistency

#### **Beneficios Estratégicos**

- **Validación de Integración Real**: Detectan problemas que las pruebas unitarias no pueden identificar
- **Configuración de Persistencia**: Validan que el mapeo ORM funcione correctamente
- **Comportamiento Transaccional**: Verifican que las transacciones mantengan la consistencia

### 2.4 Pruebas Funcionales (End-to-End)

#### **Propósito Conceptual**

Validan el **sistema completo desde la perspectiva del consumidor**, asegurando que todos los componentes integrados proporcionen la funcionalidad esperada a través de las interfaces externas.

#### **Áreas de Aplicación**

- Endpoints HTTP de la API REST
- Flujos de autenticación y autorización
- Serialización/deserialización JSON
- Códigos de estado HTTP
- Manejo de errores de API

#### **Beneficios Estratégicos**

- **Validación de Contratos**: Aseguran que la API mantiene sus contratos externos
- **Experiencia del Usuario**: Validan la funcionalidad desde la perspectiva del consumidor
- **Integración Completa**: Verifican que todos los componentes trabajen juntos correctamente

## 3. Enfoque Adoptado

### 3.1 Organización Arquitectónica

#### **Estructura Modular por Capas**

```
test/
├── Conaprole.Orders.Domain.UnitTests/           # 🔵 Capa de Dominio
├── Conaprole.Orders.Application.UnitTests/      # 🟢 Capa de Aplicación  
├── Conaprole.Orders.Application.IntegrationTests/ # 🟡 Pruebas de Integración
└── Conaprole.Orders.Api.FunctionalTests/        # 🔴 Pruebas Funcionales
```

Esta organización refleja la **arquitectura en capas** del sistema de producción, facilitando:

- **Localización Inmediata**: Los desarrolladores pueden encontrar las pruebas correspondientes a cada capa
- **Evolución Independiente**: Cada proyecto de pruebas puede evolucionar según las necesidades de su capa
- **Separación de Dependencias**: Diferentes niveles de prueba requieren diferentes dependencias y configuraciones

#### **Correspondencia Arquitectónica**

Cada proyecto de pruebas mantiene una **correspondencia directa** con la estructura del código de producción, reflejando la organización por features y agregados.

### 3.2 Patrones de Diseño Implementados

#### **Patrón AAA (Arrange-Act-Assert)**

Todas las pruebas siguen consistentemente la estructura AAA, proporcionando:

- **Claridad**: Separación clara de las fases de preparación, ejecución y verificación
- **Mantenibilidad**: Estructura predecible que facilita la comprensión y modificación
- **Legibilidad**: Código de prueba que actúa como documentación del comportamiento

```csharp
[Fact]
public void Create_Order_Should_Have_Created_Status()
{
    // Arrange - Preparación del contexto
    var pointOfSale = CreatePointOfSale();
    var distributor = CreateDistributor();
    var address = new Address("Montevideo", "18 de Julio", "11000");

    // Act - Ejecución de la operación
    var order = Order.Create(pointOfSale, distributor, address);

    // Assert - Verificación del resultado
    order.Status.Should().Be(Status.Created);
}
```

#### **Patrón Test Data Builder**

Implementación de factories especializadas para crear objetos de prueba:

- **Reutilización**: Eliminación de duplicación en la preparación de datos de prueba
- **Flexibilidad**: Capacidad de crear variaciones de objetos según necesidades específicas
- **Mantenibilidad**: Centralización de la lógica de creación de objetos de prueba

#### **Patrón Object Mother**

Utilización de clases especializadas que proporcionan objetos preconstruidos para escenarios comunes:

- **Consistencia**: Garantiza que las pruebas utilicen datos coherentes
- **Expresividad**: Métodos con nombres descriptivos que indican el propósito del objeto
- **Eficiencia**: Reducción del código repetitivo en la preparación de pruebas

### 3.3 Manejo de Dependencias

#### **Estrategia de Aislamiento**

- **Pruebas Unitarias**: Uso de mocks y stubs para aislar completamente la unidad bajo prueba
- **Pruebas de Integración**: Utilización de contenedores reales con configuración controlada
- **Pruebas Funcionales**: Sistema completo con servicios externos reemplazados por implementaciones de prueba

#### **Inyección de Dependencias en Testing**

Configuración especializada del contenedor IoC para diferentes contextos de prueba:

- **Reemplazo de Servicios Externos**: Substitución de servicios reales por implementaciones de prueba
- **Configuración de Base de Datos**: Utilización de bases de datos en memoria o contenedores
- **Mock de Autenticación**: Desactivación de autenticación real para facilitar las pruebas

#### **Gestión de Estado**

- **Aislamiento de Pruebas**: Cada prueba ejecuta en su propio contexto limpio
- **Limpieza Automática**: Mecanismos automáticos de limpieza entre pruebas
- **Determinismo**: Garantía de que las pruebas produzcan resultados consistentes

### 3.4 Separación Funcional

#### **Pruebas Unitarias vs. Funcionales**

- **Unitarias**: Enfocan en comportamientos específicos de componentes aislados
- **Funcionales**: Validan funcionalidad completa desde la perspectiva externa
- **Complementariedad**: Cada tipo cubre aspectos diferentes pero complementarios del sistema

#### **Estrategia de Cobertura**

- **Base Amplia**: Gran cantidad de pruebas unitarias para cobertura detallada
- **Integración Selectiva**: Pruebas de integración para escenarios críticos
- **Funcional Focalizada**: Pruebas funcionales para los principales flujos de usuario

## 4. Herramientas Utilizadas

### 4.1 Framework de Testing Principal

#### **xUnit.net**

**Propósito**: Framework de testing unitario para .NET

**Justificación Técnica**:

- **Integración Nativa**: Soporte completo en el ecosistema .NET
- **Paralelización**: Ejecución paralela de pruebas para mejor rendimiento
- **Extensibilidad**: Arquitectura extensible que permite personalización
- **Teoria-Based Testing**: Soporte para pruebas parametrizadas y basadas en datos

**Beneficios en CI/CD**:

- **Reportes Estándar**: Formato de resultados compatible con sistemas de CI
- **Integración con IDEs**: Soporte nativo en Visual Studio y herramientas de desarrollo
- **Escalabilidad**: Capacidad de manejar suites de pruebas grandes eficientemente

### 4.2 Assertions y Fluent Interface

#### **FluentAssertions**

**Propósito**: Biblioteca de assertions expresivas y legibles

**Justificación Técnica**:

- **Expresividad**: Sintaxis fluida que mejora la legibilidad del código de prueba
- **Mensajes Descriptivos**: Mensajes de error detallados que facilitan la depuración
- **Extensibilidad**: Capacidad de crear assertions personalizadas para el dominio
- **Comparaciones Complejas**: Soporte avanzado para comparación de objetos complejos

**Ejemplo de Uso**:

```csharp
order.Should().NotBeNull();
order.Status.Should().Be(Status.Created);
order.OrderLines.Should().HaveCount(2);
order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
```

### 4.3 Mocking y Stubbing

#### **NSubstitute**

**Propósito**: Framework de mocking para crear objetos simulados

**Justificación Técnica**:

- **Sintaxis Simple**: API intuitiva que facilita la creación de mocks
- **Verificación Avanzada**: Capacidades robustas de verificación de interacciones
- **Configuración Flexible**: Múltiples formas de configurar comportamientos simulados
- **Integración con Testing**: Diseñado específicamente para testing unitario

**Beneficios**:

- **Aislamiento Efectivo**: Permite probar unidades en completo aislamiento
- **Control de Comportamiento**: Simulación precisa de dependencias externas
- **Verificación de Interacciones**: Validación de que las colaboraciones ocurren correctamente

### 4.4 Testing de Integración y Contenedores

#### **TestContainers**

**Propósito**: Biblioteca para ejecutar contenedores Docker en pruebas

**Justificación Técnica**:

- **Realismo**: Utilización de servicios reales (PostgreSQL, Keycloak) en lugar de simulaciones
- **Aislamiento**: Cada prueba ejecuta con su propio conjunto de servicios
- **Configuración Automática**: Gestión automática del ciclo de vida de contenedores
- **Portabilidad**: Funcionamiento consistente en diferentes entornos

**Beneficios en CI/CD**:

- **Consistencia**: Mismo comportamiento en desarrollo, CI y producción
- **Paralelización**: Múltiples suites de pruebas pueden ejecutar simultáneamente
- **Limpieza Automática**: Eliminación automática de recursos después de las pruebas

### 4.5 Testing Web y API

#### **Microsoft.AspNetCore.Mvc.Testing**

**Propósito**: Framework para testing de aplicaciones ASP.NET Core

**Justificación Técnica**:

- **In-Memory Server**: Servidor web completo ejecutando en memoria
- **Configuración Especializada**: Capacidad de configurar el host para testing
- **Cliente HTTP Integrado**: Cliente preconfigurado para realizar solicitudes
- **Middleware Testing**: Capacidad de probar middleware y pipeline completo

**Beneficios**:

- **Testing Realista**: Pruebas que ejercitan el stack completo de ASP.NET Core
- **Rendimiento**: Ejecución rápida sin necesidad de servidores externos
- **Depuración**: Capacidad de depurar directamente en las pruebas

### 4.6 Cobertura de Código

#### **Coverlet**

**Propósito**: Herramienta de cobertura de código para .NET

**Justificación Técnica**:

- **Integración Nativa**: Soporte directo en el SDK de .NET
- **Formatos Múltiples**: Soporte para diversos formatos de reporte (XML, JSON, HTML)
- **CI/CD Integration**: Integración con sistemas de CI para reportes automáticos
- **Precisión**: Medición precisa de cobertura de líneas, branches y métodos

**Beneficios en Flujo de Desarrollo**:

- **Métricas Objetivas**: Datos cuantitativos sobre la calidad de las pruebas
- **Identificación de Gaps**: Detección de áreas sin cobertura de pruebas
- **Tendencias**: Seguimiento de la evolución de la cobertura a lo largo del tiempo

## 5. Beneficios Estratégicos en CI/CD

### 5.1 Integración Continua

#### **Feedback Inmediato**

La arquitectura de pruebas proporciona retroalimentación rápida en el pipeline de CI:

- **Pruebas Unitarias**: Ejecución en menos de 30 segundos
- **Pruebas de Integración**: Completadas en 2-3 minutos
- **Pruebas Funcionales**: Finalizadas en 5-10 minutos

#### **Calidad del Código**

- **Prevención de Regresiones**: Detección automática de cambios que rompen funcionalidad existente
- **Estándares de Calidad**: Mantenimiento automático de umbrales de cobertura de código
- **Documentación Viva**: Las pruebas actúan como especificación ejecutable del sistema

### 5.2 Despliegue Continuo

#### **Confianza en Releases**

La suite completa de pruebas proporciona confianza para despliegues automáticos:

- **Validación Completa**: Verificación de funcionalidad en múltiples niveles
- **Detección Temprana**: Identificación de problemas antes de llegar a producción
- **Rollback Seguro**: Capacidad de revertir cambios con confianza

#### **Automatización de QA**

- **Reducción de Testing Manual**: Automatización de casos de prueba repetitivos
- **Consistencia**: Ejecución idéntica de pruebas en cada build
- **Escalabilidad**: Capacidad de manejar incremento en la complejidad del sistema


## Conclusión

La arquitectura de pruebas del proyecto Conaprole API Core representa una implementación madura y estratégica del aseguramiento de la calidad, fundamentada en principios sólidos de ingeniería de software y mejores prácticas de la industria.

### Fortalezas Arquitectónicas

- **Cobertura Integral**: Validación completa desde la lógica de dominio hasta interfaces externas
- **Separación de Responsabilidades**: Cada tipo de prueba tiene un propósito específico y bien definido
- **Herramientas Modernas**: Utilización de tecnologías actuales y maduras del ecosistema .NET
- **Automatización Completa**: Integración seamless con pipelines de CI/CD
- **Mantenibilidad**: Código de prueba estructurado y fácil de evolucionar

### Impacto en la Calidad del Software

Esta arquitectura contribuye significativamente a:

- **Confiabilidad del Sistema**: Detección proactiva de defectos
- **Velocidad de Desarrollo**: Feedback rápido que acelera el ciclo de desarrollo
- **Evolución Segura**: Capacidad de refactorizar y evolucionar con confianza
- **Documentación Técnica**: Especificaciones ejecutables que documentan el comportamiento del sistema

La implementación demuestra un compromiso con la excelencia técnica y proporciona una base sólida para el crecimiento y evolución sostenible del sistema Conaprole Orders API.

---

*Este documento complementa la [Estrategia de Testing](../architecture/testing-strategy.md) con enfoque en principios arquitectónicos y decisiones estratégicas de calidad.*
