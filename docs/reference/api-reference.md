# 📚 API Reference

## Purpose

This document provides comprehensive API reference documentation for the Conaprole Orders REST API, including endpoints, request/response schemas, authentication, and usage examples.

## Base Information

### API Base URL
- **Production**: `https://api.conaprole.com`
- **Staging**: `https://staging-api.conaprole.com`
- **Development**: `http://localhost:5000`

### API Version
- **Current Version**: v1
- **Versioning Strategy**: URL path versioning (`/api/v1/`)
- **Content Type**: `application/json`
- **Character Encoding**: UTF-8

### Interactive Documentation
- **Swagger UI**: `{base_url}/swagger`
- **OpenAPI Spec**: `{base_url}/swagger/v1/swagger.json`

## Authentication

### Bearer Token Authentication

The API uses JWT Bearer tokens for authentication. Include the token in the Authorization header:

```http
Authorization: Bearer {jwt_token}
```

### Getting an Access Token

#### Client Credentials Flow (System-to-System)
```http
POST /auth/realms/conaprole/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={your_client_id}
&client_secret={your_client_secret}
&scope=orders:read orders:write
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cC...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "orders:read orders:write"
}
```

## Core Endpoints

### Orders API

#### Get Orders
```http
GET /api/orders
```

**Parameters:**
- `page` (query, optional): Page number (default: 1)
- `pageSize` (query, optional): Items per page (default: 20, max: 100)
- `status` (query, optional): Filter by order status
- `distributorId` (query, optional): Filter by distributor

**Response:**
```json
{
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "distributorId": "456e7890-e89b-12d3-a456-426614174001",
      "pointOfSaleId": "789e0123-e89b-12d3-a456-426614174002",
      "status": "created",
      "createdOnUtc": "2025-01-02T10:30:00Z",
      "deliveryAddress": {
        "street": "Av. 18 de Julio 1234",
        "city": "Montevideo",
        "postalCode": "11200",
        "country": "Uruguay"
      },
      "orderLines": [
        {
          "id": "order-line-uuid",
          "productId": "product-uuid",
          "quantity": {
            "value": 50,
            "unit": "liter"
          },
          "unitPrice": {
            "amount": 45.00,
            "currency": "UYU"
          },
          "totalPrice": {
            "amount": 2250.00,
            "currency": "UYU"
          }
        }
      ],
      "price": {
        "amount": 2250.00,
        "currency": "UYU"
      }
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

#### Create Order
```http
POST /api/orders
```

**Request Body:**
```json
{
  "distributorId": "456e7890-e89b-12d3-a456-426614174001",
  "pointOfSaleId": "789e0123-e89b-12d3-a456-426614174002",
  "deliveryAddress": {
    "street": "Av. 18 de Julio 1234",
    "city": "Montevideo",
    "postalCode": "11200",
    "country": "Uruguay"
  },
  "orderLines": [
    {
      "productId": "product-uuid",
      "quantity": {
        "value": 50,
        "unit": "liter"
      }
    }
  ]
}
```

**Response:** `201 Created`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "distributorId": "456e7890-e89b-12d3-a456-426614174001",
  "pointOfSaleId": "789e0123-e89b-12d3-a456-426614174002",
  "status": "created",
  "createdOnUtc": "2025-01-02T10:30:00Z",
  "price": {
    "amount": 2250.00,
    "currency": "UYU"
  }
}
```

#### Get Order by ID
```http
GET /api/orders/{orderId}
```

**Response:** `200 OK` (same schema as create response with full details)

#### Update Order Status
```http
PUT /api/orders/{orderId}/status
```

**Request Body:**
```json
{
  "status": "confirmed"
}
```

**Allowed Status Transitions:**
- `created` → `confirmed`
- `confirmed` → `in_transit`
- `in_transit` → `delivered`
- `created|confirmed` → `cancelled`

### Products API

#### Get Products
```http
GET /api/products
```

**Parameters:**
- `page`, `pageSize` (pagination)
- `active` (query, optional): Filter by active status
- `category` (query, optional): Filter by product category

**Response:**
```json
{
  "data": [
    {
      "id": "product-uuid",
      "name": "Leche Entera 1L",
      "description": "Leche entera pasteurizada",
      "externalId": "SAP_12345",
      "category": "dairy",
      "basePrice": {
        "amount": 45.00,
        "currency": "UYU"
      },
      "unit": "liter",
      "isActive": true
    }
  ],
  "pagination": { "..." }
}
```

#### Get Product by ID
```http
GET /api/products/{productId}
```

### Users API

#### Get Current User
```http
GET /api/users/me
```

**Response:**
```json
{
  "id": "user-uuid",
  "email": "user@distributor.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["distributor"],
  "permissions": ["orders:read", "orders:write"],
  "isActive": true
}
```

#### Get Users (Admin only)
```http
GET /api/users
```

**Parameters:**
- Standard pagination parameters
- `role` (query, optional): Filter by role

### Distributors API

#### Get Distributors
```http
GET /api/distributors
```

**Response:**
```json
{
  "data": [
    {
      "id": "distributor-uuid",
      "name": "Distribuidor Norte",
      "contactEmail": "contacto@distnorte.com.uy",
      "contactPhone": "+598 2XXX XXXX",
      "territories": [
        {
          "region": "Montevideo",
          "city": "Montevideo",
          "postalCodes": ["11200", "11300", "11400"]
        }
      ],
      "isActive": true
    }
  ],
  "pagination": { "..." }
}
```

### Points of Sale API

#### Get Points of Sale
```http
GET /api/points-of-sale
```

**Parameters:**
- `distributorId` (query, optional): Filter by assigned distributor

**Response:**
```json
{
  "data": [
    {
      "id": "pos-uuid",
      "name": "Supermercado Norte",
      "address": {
        "street": "Av. 18 de Julio 1234",
        "city": "Montevideo",
        "postalCode": "11200",
        "country": "Uruguay"
      },
      "contactEmail": "orders@supernorte.com.uy",
      "contactPhone": "+598 2XXX XXXX",
      "assignedDistributorId": "distributor-uuid",
      "isActive": true
    }
  ],
  "pagination": { "..." }
}
```

## Response Schemas

### Standard Response Format

All API responses follow a consistent format:

#### Success Response
```json
{
  "data": { /* response data */ },
  "pagination": { /* pagination info (for list endpoints) */ }
}
```

#### Error Response
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "orderLines[0].quantity.value",
        "message": "Quantity must be greater than 0"
      }
    ],
    "traceId": "trace-uuid"
  }
}
```

### Common Data Types

#### Money Object
```json
{
  "amount": 45.00,
  "currency": "UYU"
}
```

#### Quantity Object
```json
{
  "value": 50,
  "unit": "liter"
}
```

#### Address Object
```json
{
  "street": "Av. 18 de Julio 1234",
  "city": "Montevideo",
  "postalCode": "11200",
  "country": "Uruguay"
}
```

#### Pagination Object
```json
{
  "page": 1,
  "pageSize": 20,
  "totalItems": 150,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## HTTP Status Codes

| Status Code | Description | Usage |
|-------------|-------------|-------|
| `200 OK` | Success | GET, PUT requests |
| `201 Created` | Resource created | POST requests |
| `204 No Content` | Success, no content | DELETE requests |
| `400 Bad Request` | Invalid request | Validation errors |
| `401 Unauthorized` | Authentication required | Missing/invalid token |
| `403 Forbidden` | Access denied | Insufficient permissions |
| `404 Not Found` | Resource not found | Invalid ID |
| `409 Conflict` | Resource conflict | Business rule violation |
| `422 Unprocessable Entity` | Validation error | Business validation failed |
| `429 Too Many Requests` | Rate limit exceeded | Too many requests |
| `500 Internal Server Error` | Server error | Unexpected errors |

## Rate Limiting

### Rate Limits by Role

| Role | Limit | Window |
|------|-------|--------|
| `admin` | 1000 requests | per hour |
| `sales_manager` | 500 requests | per hour |
| `distributor` | 200 requests | per hour |
| `point_of_sale` | 100 requests | per hour |
| Anonymous | 50 requests | per hour |

### Rate Limit Headers

```http
X-RateLimit-Limit: 200
X-RateLimit-Remaining: 185
X-RateLimit-Reset: 1704196800
```

## Webhooks (Coming Soon)

Future support for webhook notifications:

### Supported Events
- `order.created`
- `order.confirmed`
- `order.delivered`
- `order.cancelled`

### Webhook Payload Example
```json
{
  "event": "order.confirmed",
  "timestamp": "2025-01-02T10:30:00Z",
  "data": {
    "orderId": "order-uuid",
    "distributorId": "distributor-uuid",
    "status": "confirmed"
  }
}
```

## SDK and Client Libraries

### Official SDKs (Planned)
- **.NET SDK**: NuGet package for .NET applications
- **Python SDK**: PyPI package for Python applications
- **JavaScript SDK**: npm package for Node.js applications

### Community Libraries
Currently accepting community contributions for client libraries.

## Testing

### Health Checks
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.123",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.045"
    },
    "keycloak": {
      "status": "Healthy",
      "duration": "00:00:00.078"
    }
  }
}
```

### Staging Environment
Use the staging environment for integration testing:
- **Base URL**: `https://staging-api.conaprole.com`
- **Test Data**: Pre-populated with sample data
- **Reset Schedule**: Daily at 02:00 UTC

## Support

### Getting Help
1. **Interactive Docs**: Use Swagger UI for real-time API testing
2. **Status Page**: Monitor system status at `/health`
3. **Error Tracing**: Include `traceId` from error responses when reporting issues

### Contact Information
- **Technical Support**: development-team@conaprole.com
- **Business Questions**: business-analysis@conaprole.com
- **Security Issues**: security@conaprole.com

---

*Last verified: 2025-01-02 - Commit: [documentation restructure]*