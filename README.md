# 🛒 Conaprole Orders API

[![Documentation](https://github.com/ingnlema/Conaprole/workflows/Documentation%20Validation/badge.svg)](https://github.com/ingnlema/Conaprole/actions/workflows/docs.yml)
[![Build and Deploy](https://github.com/ingnlema/Conaprole/workflows/Deploy%20to%20Azure%20Container%20Apps/badge.svg)](https://github.com/ingnlema/Conaprole/actions/workflows/main.yml)

Modern order management system built with .NET 8, Clean Architecture, and Domain-Driven Design principles.

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) for local development
- [PostgreSQL](https://www.postgresql.org/) database
- [Keycloak](https://www.keycloak.org/) for authentication

### Local Development

```bash
# Clone the repository
git clone https://github.com/ingnlema/Conaprole.git
cd Conaprole

# Restore dependencies
dotnet restore

# Start local infrastructure
docker-compose up -d

# Run database migrations
dotnet ef database update --project src/Conaprole.Orders.Infrastructure

# Start the API
dotnet run --project src/Conaprole.Orders.Api
```

The API will be available at `https://localhost:7017` with Swagger documentation at `/swagger`.

## 📚 Documentation

Comprehensive documentation is available in the [`docs/`](./docs/) directory:

- **[📖 Documentation Index](./docs/README.md)** - Start here for navigation
- **[🏗️ Architecture](./docs/architecture/README.md)** - System design and patterns
- **[🔐 Security](./docs/security/README.md)** - Authentication and authorization
- **[🧪 Testing](./docs/testing/README.md)** - Testing strategies and guides
- **[❓ FAQ](./docs/FAQ.md)** - Frequently asked questions

### Quick Links

| Topic | Documentation | Description |
|-------|---------------|-------------|
| **Getting Started** | [System Overview](./docs/overview/README.md) | High-level introduction |
| **API Design** | [API Design Patterns](./docs/architecture/api-design.md) | REST API conventions |
| **Authentication** | [Auth Setup](./docs/security/authentication.md) | JWT + Keycloak integration |
| **Testing** | [Test Setup](./docs/testing/integration-tests-setup.md) | Running tests locally |
| **Deployment** | [Azure Deployment](./docs/how-to/README.md) | Production deployment |

## 🏗️ Architecture

The system follows **Clean Architecture** principles with these layers:

```
┌─────────────────────────────────────┐
│             API Layer               │  ← Controllers, DTOs, Swagger
├─────────────────────────────────────┤
│         Application Layer           │  ← Use Cases, CQRS, Handlers
├─────────────────────────────────────┤
│           Domain Layer              │  ← Entities, Value Objects, Rules
├─────────────────────────────────────┤
│        Infrastructure Layer         │  ← Database, Auth, External APIs
└─────────────────────────────────────┘
```

**Key Technologies:**
- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
- **Database**: PostgreSQL with EF Core migrations
- **Authentication**: JWT tokens with Keycloak
- **Testing**: xUnit, TestContainers, FluentAssertions
- **Documentation**: Markdown with Mermaid diagrams

## 🔐 Security

- **Authentication**: JWT bearer tokens from Keycloak
- **Authorization**: Role and permission-based access control
- **API Security**: HTTPS, CORS, input validation
- **Database**: Connection string encryption, parameterized queries

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests by category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration  
dotnet test --filter Category=Functional

# Generate test coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Coverage:**
- **Unit Tests**: Domain logic and business rules
- **Integration Tests**: Database and repository layers
- **Functional Tests**: API endpoints and workflows

## 📊 API Endpoints

| Endpoint | Description | Authentication |
|----------|-------------|----------------|
| `GET /api/users` | List users | `users:read` |
| `POST /api/users` | Create user | `users:write` |
| `GET /api/orders` | List orders | `orders:read` |
| `POST /api/orders` | Create order | `orders:write` |
| `GET /api/products` | List products | `products:read` |
| `POST /api/products` | Create product | `products:write` |

Full API documentation available at `/swagger` when running locally.

## 🚀 Deployment

The application is deployed to **Azure Container Apps** with automatic CI/CD via GitHub Actions.

**Production Environment:**
- **API**: Azure Container Apps
- **Database**: Azure Database for PostgreSQL
- **Authentication**: Keycloak (containerized)
- **Monitoring**: Application Insights

## 🤝 Contributing

1. **Read the documentation** in [`docs/`](./docs/) directory
2. **Follow coding standards** outlined in [Code Conventions](./docs/architecture/convenciones-codigo.md)
3. **Write tests** for new functionality
4. **Update documentation** for any changes
5. **Submit pull requests** with clear descriptions

### Development Workflow

```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make changes and test
dotnet test
make docs-validate

# Commit and push
git commit -m "feat: add your feature"
git push origin feature/your-feature-name

# Create pull request
```

## 📈 Monitoring & Logs

- **Application Logs**: Structured logging with Serilog
- **Health Checks**: `/health`, `/health/ready`, `/health/live`
- **Metrics**: Application Insights integration
- **Error Tracking**: Automatic error logging and notifications

## 📝 Documentation Status

[![Documentation Validation](https://github.com/ingnlema/Conaprole/workflows/Documentation%20Validation/badge.svg)](https://github.com/ingnlema/Conaprole/actions/workflows/docs.yml)

- **📄 47 documentation files** with automated validation
- **🔗 148 verified internal links** 
- **💻 334 tested code snippets**
- **📊 52 architectural diagrams**
- **✅ Automated quality checks** in CI/CD

## 📞 Support

- **Documentation**: Start with [docs/README.md](./docs/README.md)
- **FAQ**: Common questions in [docs/FAQ.md](./docs/FAQ.md)
- **Issues**: Report bugs via GitHub Issues
- **Architecture**: Review [architecture docs](./docs/architecture/README.md)

## 📄 License

This project is developed for Conaprole internal use.

---

*For detailed technical documentation, visit the [docs directory](./docs/README.md).*