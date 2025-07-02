# 📚 Conaprole Orders - Documentation

## Purpose

This is the central documentation hub for the Conaprole Orders system. All technical documentation, guides, and references are organized here for easy navigation.

## Audience

- **Developers** - Implementation guides and technical references
- **Architects** - System design and architectural decisions
- **QA Engineers** - Testing strategies and test setup
- **DevOps/SysAdmins** - Deployment and operational procedures

## Quick Navigation

### 🚀 Getting Started
- [System Overview](./overview/README.md) - High-level system introduction
- [Architecture Overview](./architecture/README.md) - Technical architecture guide
- [Development Setup](./how-to/development-setup.md) - Local development environment

### 🏗️ Architecture & Design
- [Clean Architecture](./architecture/clean-architecture.md) - Architectural principles
- [Domain Design](./architecture/domain-design.md) - Domain-driven design patterns
- [CQRS & Mediator](./architecture/cqrs-mediator.md) - Command/Query separation
- [Security Architecture](./architecture/security-architecture.md) - Security design

### 🔐 Security
- [Security Overview](./security/README.md) - Security documentation index
- [Authentication](./security/authentication.md) - JWT and Keycloak integration
- [Authorization](./security/authorization.md) - Permissions and roles system
- [Implementation Guide](./security/implementation-guide.md) - Security implementation

### 🧪 Testing & Quality
- [Testing Strategy](./architecture/testing-strategy.md) - Overall testing approach
- [Integration Tests](./testing/integration-tests-setup.md) - Integration testing setup
- [Quality Architecture](./quality/arquitectura-pruebas.md) - Testing architecture
- [CI/CD Testing](./quality/automatizacion-pruebas-ci.md) - Automated testing

### 📖 Reference
- [API Design](./architecture/api-design.md) - API design patterns
- [Data Layer](./architecture/data-layer.md) - Database and ORM patterns
- [Error Handling](./architecture/manejo-errores.md) - Error handling strategies
- [Code Conventions](./architecture/convenciones-codigo.md) - Coding standards

### 📋 Use Cases & Flows
- [Use Cases Overview](./architecture/casos-de-uso/README.md) - Use case diagrams
- [Sequence Diagrams](./architecture/diagramas-secuencia/README.md) - Flow diagrams

## Documentation Structure

```
docs/
├── overview/           # System overview and introduction
├── architecture/       # Technical architecture documentation
├── security/          # Security, authentication, and authorization
├── testing/           # Testing guides and strategies
├── quality/           # Quality assurance and CI/CD
├── how-to/            # Step-by-step tutorials and guides
├── reference/         # API reference and technical specs
└── STYLE_GUIDE.md     # Documentation standards and conventions
```

## Contributing to Documentation

1. **Follow the Style Guide** - See [STYLE_GUIDE.md](./STYLE_GUIDE.md) for formatting standards
2. **Verify Code Examples** - Ensure all code snippets compile and execute
3. **Update Cross-References** - Maintain links between related documents
4. **Include Verification Info** - Add "Last verified" footer with commit SHA

## Documentation Standards

- 📝 **Consistent Formatting** - Follow markdownlint rules
- 🔗 **Valid Links** - All internal and external links must work
- 💻 **Executable Code** - All code examples must be runnable
- 📊 **Current Diagrams** - Visual representations must reflect current state
- 🏷️ **Proper Metadata** - Include purpose, audience, and prerequisites

## Recent Updates

- **2025-01-02**: Initial documentation audit and restructuring
- **Previous**: Comprehensive security analysis and authorization guides

## Getting Help

- **Issues**: Check existing documentation issues in GitHub
- **Questions**: Ask in development team channels
- **Improvements**: Submit PRs with documentation enhancements

---

*Last verified: 2025-01-02 - Commit: [initial documentation restructure]*