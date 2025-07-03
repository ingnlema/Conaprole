# 🚀 Quick Start Guide

## Purpose

This guide provides a fast track to understanding and getting started with the Conaprole Orders system. Whether you're a developer, business user, or system integrator, this guide will help you quickly orient yourself and begin working with the system.

## Prerequisites

Before getting started, ensure you have:

### For Developers
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL Client** (optional) - For database access
- **Git** - For source code access
- **Visual Studio 2022** or **VS Code** - Recommended IDEs

### For Business Users
- **Web Browser** - Chrome, Firefox, Safari, or Edge
- **API Client** (optional) - Postman or similar for API testing
- **Valid User Account** - Provided by system administrator

### For System Integrators
- **API Documentation Access** - Swagger/OpenAPI documentation
- **Integration Credentials** - OAuth client credentials
- **Network Access** - VPN or authorized IP addresses

## Quick Overview

### What is Conaprole Orders?

Conaprole Orders is a **modern order management API** that handles dairy product orders within the Conaprole distribution network. The system supports:

- **Order Management**: Create, track, and fulfill orders
- **User Management**: Role-based access control
- **Product Catalog**: Dairy product information and pricing
- **Distributor Network**: Territory-based distribution management

### Key Concepts (2-minute read)

1. **Orders**: Requests for dairy products from points of sale
2. **Distributors**: Regional distribution partners with assigned territories
3. **Points of Sale**: Retail locations that place orders
4. **Products**: Dairy products available for ordering
5. **Users**: System actors with different roles and permissions

## Getting Started by Role

### 🏢 Business Users

#### 1. Access the System
```
Production URL: https://<domain>
Staging URL: https://staging-<domain>
Documentation: https://<domain>/swagger
```

#### 2. Understanding Your Role
- **Distributors**: Can manage orders within their territory
- **Points of Sale**: Can create and track their orders
- **Sales Managers**: Can view regional performance and manage distributors

#### 3. Common Tasks

##### Creating an Order (Point of Sale)
1. Log in to the system
2. Browse the product catalog
3. Add products to your order
4. Review and submit the order
5. Track order status until delivery

##### Managing Territory (Distributor)
1. View pending orders in your territory
2. Confirm orders when ready to fulfill
3. Update order status during delivery
4. Generate reports for your region

### 👨‍💻 Developers

#### 1. Local Development Setup

```bash
# Clone the repository
git clone https://github.com/ucudal/Conaprole_BackEnd.git
cd Conaprole_BackEnd

# Start infrastructure services
docker-compose up -d postgres keycloak

# Run the application
dotnet run --project src/Conaprole.Orders.Api

# Access locally
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

#### 2. Environment Configuration

Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=conaprole_orders;Username=conaprole;Password=development"
  },
  "Authentication": {
    "Authority": "http://localhost:8080/realms/conaprole",
    "Audience": "conaprole-orders"
  }
}
```

#### 3. Running Tests

```bash
# Unit tests
dotnet test tests/Conaprole.Orders.UnitTests

# Integration tests (requires Docker)
dotnet test tests/Conaprole.Orders.IntegrationTests

# All tests
dotnet test
```

#### 4. Key API Endpoints

```http
# Health check
GET /health

# Authentication (get token from Keycloak)
POST /auth/realms/conaprole/protocol/openid-connect/token

# Orders
GET /api/orders
POST /api/orders
GET /api/orders/{id}
PUT /api/orders/{id}

# Products
GET /api/products
GET /api/products/{id}

# Users
GET /api/users/me
```

### 🔗 System Integrators

#### 1. API Authentication

##### Option A: Client Credentials (Recommended)
```bash
curl -X POST \
  'https://auth.conaprole.com/realms/conaprole/protocol/openid-connect/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'grant_type=client_credentials&client_id=your-client-id&client_secret=your-client-secret'
```

##### Option B: API Key (Legacy)
```bash
curl -X GET \
  'https://api.conaprole.com/api/orders' \
  -H 'Authorization: ApiKey your-api-key'
```

#### 2. Sample Integration Code

##### .NET Client
```csharp
public class ConaproleOrdersClient
{
    private readonly HttpClient _httpClient;
    
    public ConaproleOrdersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<Order[]> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync("/api/orders");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Order[]>();
    }
    
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Order>();
    }
}
```

##### Python Client
```python
import requests
import json

class ConaproleOrdersClient:
    def __init__(self, base_url, access_token):
        self.base_url = base_url
        self.headers = {
            'Authorization': f'Bearer {access_token}',
            'Content-Type': 'application/json'
        }
    
    def get_orders(self):
        response = requests.get(f'{self.base_url}/api/orders', headers=self.headers)
        response.raise_for_status()
        return response.json()
    
    def create_order(self, order_data):
        response = requests.post(
            f'{self.base_url}/api/orders',
            headers=self.headers,
            json=order_data
        )
        response.raise_for_status()
        return response.json()
```

#### 3. Integration Patterns

##### Webhook Notifications (Coming Soon)
```json
{
  "event_type": "order.confirmed",
  "timestamp": "2025-01-02T10:30:00Z",
  "data": {
    "order_id": "123e4567-e89b-12d3-a456-426614174000",
    "distributor_id": "456e7890-e89b-12d3-a456-426614174001",
    "status": "confirmed"
  }
}
```

## Common Use Cases

### 📋 Use Case 1: Creating an Order

#### Business Flow
1. Point of sale browses product catalog
2. Adds products to order with quantities
3. Reviews order total and delivery details
4. Submits order for processing
5. Receives order confirmation

#### API Sequence
```http
# 1. Get available products
GET /api/products?active=true

# 2. Create order
POST /api/orders
{
  "pointOfSaleId": "pos-uuid",
  "distributorId": "dist-uuid",
  "deliveryAddress": {
    "street": "Av. 18 de Julio 1234",
    "city": "Montevideo",
    "postalCode": "11200"
  },
  "orderLines": [
    {
      "productId": "prod-uuid",
      "quantity": 50
    }
  ]
}

# 3. Track order status
GET /api/orders/{orderId}
```


## Troubleshooting

### Common Issues

#### 🔧 Authentication Problems
```
Error: 401 Unauthorized
Solution: Check token expiration and scope permissions
```

#### 🔧 Validation Errors
```
Error: 400 Bad Request - Invalid quantity
Solution: Ensure quantities are positive numbers
```

#### 🔧 Territory Restrictions
```
Error: 403 Forbidden - Access denied to resource
Solution: Verify user has access to the requested territory
```

### Getting Help

1. **API Documentation**: Visit `/swagger` endpoint for detailed API docs
2. **Status Page**: Check system status at `/health`
3. **Support**: Contact development team via designated channels
4. **Documentation**: Full documentation in `docs/` directory

## Next Steps

### For Business Users
1. **Explore Features**: Try creating and managing orders
2. **Generate Reports**: Use the reporting functionality
3. **Provide Feedback**: Share user experience feedback

### For Developers
1. **Read Architecture Docs**: Understand the system design
2. **Explore Code**: Review the codebase structure
3. **Contribute**: Follow contribution guidelines

### For Integrators
1. **Test Integration**: Use staging environment for testing
2. **Monitor Performance**: Set up monitoring for your integration
3. **Plan for Scale**: Understand rate limits and best practices

## Resources

### Documentation Links
- [System Overview](./system-overview.md) - Detailed system description
- [Business Context](./business-context.md) - Business requirements and goals
- [Technology Stack](./technology-stack.md) - Technical architecture details
- [User Roles](./user-roles.md) - Role and permission system
- [Integration Points](./integration-points.md) - External system connections

### External Resources
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Docker Documentation](https://docs.docker.com/)

---

**🎯 Goal**: Get productive with Conaprole Orders in under 30 minutes!

*Last verified: 2025-01-02 - Commit: [documentation restructure]*
