# 🛠️ Technology Stack

## Purpose

This document provides a comprehensive overview of the technology stack used in the Conaprole Orders system, including justifications for technology choices, version information, and architectural considerations.

## Technology Overview

The Conaprole Orders system is built using a modern, enterprise-grade technology stack focused on scalability, maintainability, and performance. The architecture follows industry best practices and leverages proven technologies in the .NET ecosystem.

## Core Technologies

### Backend Framework

#### .NET 8
- **Version**: 8.0 LTS (Long Term Support)
- **Language**: C# 12
- **Runtime**: .NET Runtime 8.0

**Why .NET 8:**
- Long-term support with security updates until 2026
- Excellent performance improvements over previous versions
- Rich ecosystem and extensive library support
- Strong typing and compile-time error detection
- Cross-platform support (Windows, Linux, macOS)

#### ASP.NET Core Web API
- **Version**: 8.0
- **Purpose**: RESTful API development

**Features Used:**
- Built-in dependency injection
- Middleware pipeline for cross-cutting concerns
- Model binding and validation
- OpenAPI/Swagger integration
- Health checks and monitoring

### Database Technology

#### PostgreSQL
- **Version**: 15.x
- **Driver**: Npgsql 8.0

**Why PostgreSQL:**
- Open-source with enterprise features
- ACID compliance and data integrity
- Advanced indexing and query optimization
- JSON support for flexible data storage
- Excellent performance for read/write workloads
- Strong community support and ecosystem

#### Object-Relational Mapping (ORM)

##### Entity Framework Core
- **Version**: 8.0
- **Purpose**: Primary ORM for CRUD operations

**Configuration:**
```csharp
// Snake case naming convention for PostgreSQL
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());
```

##### Dapper
- **Version**: 2.1
- **Purpose**: High-performance queries for read operations

**Use Cases:**
- Complex reporting queries
- Performance-critical read operations
- Custom SQL optimization

### Authentication & Authorization

#### Keycloak
- **Version**: 22.x
- **Purpose**: Identity and Access Management (IAM)

**Features:**
- OAuth 2.0 and OpenID Connect support
- Role-based access control (RBAC)
- Single Sign-On (SSO)
- User federation and social login
- Administrative console

**Integration:**
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = "conaprole-orders";
    });
```

## Architectural Patterns

### Clean Architecture
- **Domain Layer**: Business entities and core logic
- **Application Layer**: Use cases and application services
- **Infrastructure Layer**: External concerns (database, external APIs)
- **Presentation Layer**: API controllers and DTOs

### CQRS (Command Query Responsibility Segregation)
- **Library**: MediatR 12.x
- **Purpose**: Separation of read and write operations

**Benefits:**
- Optimized query performance
- Clear separation of concerns
- Scalable read/write models
- Simplified testing

### Repository Pattern
- **Purpose**: Abstraction layer for data access
- **Implementation**: Generic base repository with specific implementations

## Development Tools & Libraries

### API Documentation
#### Swagger/OpenAPI
- **Library**: Swashbuckle.AspNetCore 6.5
- **Purpose**: Interactive API documentation

**Features:**
- Automatic API discovery
- Request/response examples
- Authentication integration
- Code generation support

### Validation
#### FluentValidation
- **Version**: 11.x
- **Purpose**: Business rule validation

**Benefits:**
- Strongly-typed validation rules
- Separation of validation from domain models
- Comprehensive validation scenarios
- Easy testing of validation logic

### Logging
#### Serilog
- **Version**: 3.x
- **Purpose**: Structured logging

**Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(telemetryConfiguration)
    .CreateLogger();
```

**Sinks:**
- Console output for development
- Azure Application Insights for production
- File logging for debugging

## Testing Framework

### Unit Testing
#### xUnit
- **Version**: 2.4.x
- **Purpose**: Primary testing framework

**Features:**
- Parallel test execution
- Dependency injection support
- Theory and fact-based tests
- Extensible architecture

#### FluentAssertions
- **Version**: 6.x
- **Purpose**: Expressive test assertions

**Benefits:**
- Readable test code
- Detailed error messages
- Extensive assertion library
- Natural language syntax

### Mocking
#### Moq
- **Version**: 4.20.x
- **Purpose**: Mock object creation for testing

### Integration Testing
#### TestContainers
- **Version**: 3.x
- **Purpose**: Docker containers for integration tests

**Benefits:**
- Real database testing
- Isolated test environments
- Consistent test data
- CI/CD pipeline integration

### Test Data Generation
#### Bogus
- **Version**: 34.x
- **Purpose**: Fake data generation for testing

## Containerization & Deployment

### Docker
- **Base Image**: mcr.microsoft.com/dotnet/aspnet:8.0
- **Purpose**: Application containerization

**Multi-stage Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Build stage

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# Runtime stage
```

### Docker Compose
- **Purpose**: Local development environment
- **Services**: API, PostgreSQL, Keycloak

## CI/CD Pipeline

### GitHub Actions
- **Purpose**: Continuous Integration and Deployment

**Workflow Stages:**
1. Code checkout
2. .NET SDK setup
3. Dependency restoration
4. Code compilation
5. Test execution
6. Docker image building
7. Security scanning
8. Deployment

## Performance & Monitoring

### Application Performance Monitoring
#### Azure Application Insights
- **Purpose**: Telemetry and performance monitoring

**Metrics Tracked:**
- Request response times
- Dependency call durations
- Exception rates and details
- Custom business metrics

### Health Checks
- **Library**: Microsoft.Extensions.Diagnostics.HealthChecks
- **Endpoints**: `/health`, `/health/ready`, `/health/live`

**Checks:**
- Database connectivity
- External service availability
- Application readiness

## Security Considerations

### Data Protection
- **Encryption**: HTTPS/TLS 1.3 for data in transit
- **Database**: PostgreSQL encryption for data at rest
- **Secrets**: Azure Key Vault for secret management

### API Security
- **Authentication**: JWT Bearer tokens
- **Authorization**: Role and permission-based
- **Rate Limiting**: Built-in rate limiting middleware
- **CORS**: Configured for specific origins

## Development Environment

### IDE Support
- **Visual Studio 2022**: Primary IDE
- **Visual Studio Code**: Alternative with C# extension
- **JetBrains Rider**: Alternative professional IDE

### Package Management
- **NuGet**: .NET package management
- **Docker Hub**: Container image registry
- **GitHub Packages**: Private package hosting

## Version Management

### Semantic Versioning
- **Format**: MAJOR.MINOR.PATCH
- **Current**: Following SemVer 2.0.0 specification

### Dependency Management
- **Strategy**: Regular updates with compatibility testing
- **Security**: Automated vulnerability scanning
- **LTS Preference**: Long-term support versions when available

## Technology Justification Summary

| Technology | Justification | Alternatives Considered |
|------------|---------------|------------------------|
| **.NET 8** | LTS, Performance, Ecosystem | Java Spring Boot, Node.js |
| **PostgreSQL** | Open source, Performance, Features | SQL Server, MySQL |
| **Keycloak** | Open source, Feature-rich, Standards-compliant | Auth0, Azure AD B2C |
| **Entity Framework Core** | .NET integration, Code-first, Migrations | Dapper only, NHibernate |
| **xUnit** | .NET standard, Parallel execution | NUnit, MSTest |
| **Docker** | Consistency, Portability, CI/CD | Native deployment, VMs |

---

*Last verified: 2025-01-02 - Commit: [documentation restructure]*