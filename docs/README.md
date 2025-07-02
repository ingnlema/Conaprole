# 📚 Documentación - Conaprole Orders API

[![Docs Build](https://img.shields.io/badge/docs-passing-brightgreen)](https://github.com/ingnlema/Conaprole)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Bienvenido** a la documentación técnica del proyecto Conaprole Orders API.  
> Esta documentación está diseñada para desarrolladores, arquitectos y equipos de QA.

## 🗺️ Navegación Rápida

| 📂 Sección | 📄 Descripción | 🎯 Audiencia |
|------------|----------------|--------------|
| [🏗️ Arquitectura](#️-arquitectura) | Patrones, diseño y estructura del sistema | Desarrolladores Senior, Arquitectos |
| [🔒 Seguridad](#-seguridad) | Autenticación, autorización y buenas prácticas | Todos los desarrolladores |
| [🧪 Testing](#-testing) | Estrategias de pruebas y guías de QA | QA Engineers, Desarrolladores |
| [⚡ Quality](#-quality) | Métricas, CI/CD y aseguramiento de calidad | DevOps, Tech Leads |

---

## 🏗️ Arquitectura

### 📋 Documentos Fundamentales

- [**📐 Clean Architecture**](architecture/clean-architecture.md) - Principios y capas del sistema
- [**🎯 CQRS & MediatR**](architecture/cqrs-mediator.md) - Patrones de comando y consulta
- [**📝 Convenciones de Código**](architecture/convenciones-codigo.md) - Estándares de desarrollo
- [**🌐 Diseño de API**](architecture/api-design.md) - Patrones REST y endpoints

### 🎨 Diagramas y Casos de Uso

- [**📊 Casos de Uso**](architecture/casos-de-uso/) - Diagramas por actor del sistema
- [**🔄 Diagramas de Secuencia**](architecture/diagramas-secuencia/) - Flujos de comando y consulta
- [**🏛️ Domain Design**](architecture/domain-design.md) - Modelado del dominio

### 🔧 Patrones Técnicos

- [**💉 Dependency Injection**](architecture/dependency-injection.md) - Configuración de DI
- [**🗄️ Data Layer**](architecture/data-layer.md) - Acceso a datos y Entity Framework
- [**⚠️ Manejo de Errores**](architecture/manejo-errores.md) - Estrategias de error handling

---

## 🔒 Seguridad

### 📋 Guías Esenciales

- [**🔐 Authentication**](security/authentication.md) - Integración con Keycloak
- [**🛡️ Authorization**](security/authorization.md) - Permisos y roles
- [**🔍 Security Architecture**](security/security-architecture.md) - Arquitectura de seguridad

### 📊 Análisis y Planes

- [**📈 Authorization Analysis**](security/authorization-analysis.md) - Análisis exhaustivo del sistema actual
- [**🔧 Refactoring Plan**](security/authorization-refactoring-plan.md) - Plan de mejoras de autorización
- [**⚡ Immediate Actions**](security/immediate-actions.md) - Acciones prioritarias

---

## 🧪 Testing

### 📋 Estrategias de Pruebas

- [**🏗️ Testing Strategy**](architecture/testing-strategy.md) - Enfoque general de testing
- [**⚙️ Integration Tests Setup**](testing/integration-tests-setup.md) - Configuración de pruebas de integración

### 📊 Cobertura Actual

- [**📋 Authorization Tests**](../test/Conaprole.Orders.Api.FunctionalTests/Authorization/README.md) -
  Cobertura de tests de autorización

---

## ⚡ Quality

### 📋 Aseguramiento de Calidad

- [**🏗️ Arquitectura de Pruebas**](quality/arquitectura-pruebas.md) - Framework y herramientas de testing
- [**🤖 Automatización CI/CD**](quality/automatizacion-pruebas-ci.md) - Pipeline de integración continua
- [**📝 Casos de Prueba**](quality/casos-de-prueba.md) - Escenarios de testing

---

## 🚀 Inicio Rápido

### Para Nuevos Desarrolladores

1. **📖 Lee la introducción**: [Clean Architecture](architecture/clean-architecture.md)
2. **🔧 Configura el ambiente**: [Integration Tests Setup](testing/integration-tests-setup.md)
3. **🔐 Entiende la seguridad**: [Authorization](security/authorization.md)
4. **📝 Sigue las convenciones**: [Convenciones de Código](architecture/convenciones-codigo.md)

### Para Arquitectos

1. **🏛️ Revisa el diseño**: [Domain Design](architecture/domain-design.md)
2. **📊 Analiza los diagramas**: [Casos de Uso](architecture/casos-de-uso/)
3. **🔍 Evalúa la seguridad**: [Security Architecture](security/security-architecture.md)
4. **⚡ Optimiza el CI/CD**: [Automatización de Pruebas](quality/automatizacion-pruebas-ci.md)

### Para QA Engineers

1. **🧪 Comprende la estrategia**: [Testing Strategy](architecture/testing-strategy.md)
2. **📋 Revisa los casos**: [Casos de Prueba](quality/casos-de-prueba.md)
3. **🔐 Prueba la autorización**: [Authorization Tests](../test/Conaprole.Orders.Api.FunctionalTests/Authorization/README.md)

---

## 📞 Soporte y Contribución

### 🤝 Cómo Contribuir

1. **📝 Plantilla**: Usa [la plantilla estándar](_TEMPLATE.md) para nuevos documentos
2. **✅ Validación**: Ejecuta `markdownlint` antes de enviar cambios
3. **🔍 Verificación**: Asegúrate que los ejemplos de código compilen

### 📋 Guías de Estilo

- **Idioma**: Español para documentación, inglés para código
- **Formato**: Markdown con Mermaid para diagramas
- **Longitud**: Máximo 120 caracteres por línea
- **Estructura**: Sigue la plantilla estándar

### ❓ FAQ

#### ¿Cómo genero los diagramas localmente?

```bash
# Instalar Mermaid CLI
npm install -g @mermaid-js/mermaid-cli

# Generar diagrama
mmdc -i diagram.md -o diagram.png
```

#### ¿Cómo verifico que mis ejemplos compilan?

```bash
# Ejecutar build completo
dotnet build Conaprole.Orders.sln --configuration Release

# Ejecutar tests
dotnet test --configuration Release
```

#### ¿Cómo valido el formato de la documentación?

```bash
# Validar Markdown
markdownlint-cli2 "docs/**/*.md"

# Auto-formatear
markdownlint-cli2 "docs/**/*.md" --fix
```

---

> **Última actualización**: 2025-07-02  
> **Versión de la documentación**: 1.0  
> **Mantenido por**: @ingnlema, @FernandoMachado
