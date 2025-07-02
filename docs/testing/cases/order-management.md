---
title: "Casos de Prueba - Gestión de Pedidos"
description: "Casos de prueba detallados para el módulo de gestión de pedidos incluyendo resultados de ejecución y evidencias"
last_verified_sha: "4ef981b"
---

# 📋 Casos de Prueba - Gestión de Pedidos

## TC-002: Order Management Test Cases

### TC-002-01: Crear Pedido Válido

| Campo | Valor |
|-------|-------|
| **ID** | TC-002-01 |
| **Título** | Crear pedido con datos válidos |
| **Módulo** | Order Management |
| **Prioridad** | Crítica |
| **Categoría** | Functional |
| **Método** | Automated |

#### Prerequisitos
- Usuario autenticado con permiso `orders:write`
- Distribuidor activo en sistema
- Punto de venta configurado
- Al menos un producto disponible

#### Datos de Prueba
```json
{
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "pointOfSaleId": "223e4567-e89b-12d3-a456-426614174001", 
  "distributorId": "323e4567-e89b-12d3-a456-426614174002",
  "deliveryAddress": {
    "street": "Av. 18 de Julio 1234",
    "city": "Montevideo",
    "postalCode": "11000"
  },
  "orderLines": [
    {
      "productId": "423e4567-e89b-12d3-a456-426614174003",
      "quantity": 2
    }
  ]
}
```

#### Pasos de Ejecución
1. **Setup**: Autenticar usuario con token JWT válido
2. **Action**: Enviar POST request a `/api/orders` con datos válidos
3. **Verification**: Verificar response status y body
4. **Cleanup**: Verificar persistencia en base de datos

#### Resultado Esperado
- **HTTP Status**: 201 Created
- **Response Body**: Contiene order ID, status "Draft", totales calculados
- **Headers**: Location header apunta a nuevo recurso
- **Database**: Registro creado con datos correctos

#### Criterios de Aceptación
- ✅ Order ID generado (UUID válido)
- ✅ Status inicial = "Draft"
- ✅ Totales calculados correctamente
- ✅ OrderLines relacionadas correctamente
- ✅ Timestamps poblados (CreatedOnUtc)
- ✅ Relaciones FK válidas

#### Resultado de Ejecución

| Fecha | Ambiente | Resultado | Ejecutado Por | Notas |
|-------|----------|-----------|---------------|-------|
| 2025-01-02 | TEST | ✅ PASS | QA Engineer | Todos los criterios cumplidos |
| 2025-01-01 | DEV | ✅ PASS | Developer | Test automatizado exitoso |

#### Evidencias
```bash
# Request
POST /api/orders
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

# Response
HTTP/1.1 201 Created
Location: /api/orders/523e4567-e89b-12d3-a456-426614174004
Content-Type: application/json

{
  "id": "523e4567-e89b-12d3-a456-426614174004",
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Draft",
  "totalAmount": {
    "amount": 20.00,
    "currency": "USD"
  },
  "createdOnUtc": "2025-01-02T10:30:00Z",
  "orderLines": [
    {
      "productId": "423e4567-e89b-12d3-a456-426614174003",
      "quantity": 2,
      "unitPrice": {
        "amount": 10.00,
        "currency": "USD"
      },
      "subtotal": {
        "amount": 20.00,
        "currency": "USD"
      }
    }
  ]
}
```

---

### TC-002-02: Confirmar Pedido Draft

| Campo | Valor |
|-------|-------|
| **ID** | TC-002-02 |
| **Título** | Confirmar pedido en estado Draft |
| **Módulo** | Order Management |
| **Prioridad** | Alta |
| **Categoría** | Functional |
| **Método** | Automated |

#### Prerequisitos
- Pedido existente en estado "Draft"
- Usuario con permiso `orders:write`

#### Datos de Prueba
```json
{
  "orderId": "523e4567-e89b-12d3-a456-426614174004"
}
```

#### Pasos de Ejecución
1. **Setup**: Crear pedido en estado Draft
2. **Action**: Enviar PUT request a `/api/orders/{id}/confirm`
3. **Verification**: Verificar cambio de estado
4. **Validation**: Confirmar que no se puede modificar

#### Resultado Esperado
- **HTTP Status**: 200 OK
- **Response**: Status cambiado a "Confirmed"
- **Database**: ConfirmedOnUtc poblado
- **Business Rule**: Pedido no modificable

#### Resultado de Ejecución

| Fecha | Ambiente | Resultado | Ejecutado Por | Notas |
|-------|----------|-----------|---------------|-------|
| 2025-01-02 | TEST | ✅ PASS | QA Engineer | Estado cambiado correctamente |

---

### TC-002-03: Crear Pedido Sin Líneas

| Campo | Valor |
|-------|-------|
| **ID** | TC-002-03 |
| **Título** | Intentar crear pedido sin líneas de pedido |
| **Módulo** | Order Management |
| **Prioridad** | Alta |
| **Categoría** | Negative |
| **Método** | Automated |

#### Datos de Prueba
```json
{
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "pointOfSaleId": "223e4567-e89b-12d3-a456-426614174001",
  "distributorId": "323e4567-e89b-12d3-a456-426614174002",
  "deliveryAddress": {
    "street": "Av. 18 de Julio 1234",
    "city": "Montevideo", 
    "postalCode": "11000"
  },
  "orderLines": []
}
```

#### Resultado Esperado
- **HTTP Status**: 400 Bad Request
- **Error Message**: "At least one order line is required"
- **Database**: Ningún registro creado

#### Resultado de Ejecución

| Fecha | Ambiente | Resultado | Ejecutado Por | Notas |
|-------|----------|-----------|---------------|-------|
| 2025-01-02 | TEST | ✅ PASS | QA Engineer | Validación correcta |

---

### TC-002-04: Modificar Pedido Confirmado

| Campo | Valor |
|-------|-------|
| **ID** | TC-002-04 |
| **Título** | Intentar modificar pedido en estado Confirmed |
| **Módulo** | Order Management |
| **Prioridad** | Alta |
| **Categoría** | Business Rule |
| **Método** | Automated |

#### Prerequisitos
- Pedido existente en estado "Confirmed"

#### Pasos de Ejecución
1. **Setup**: Crear y confirmar pedido
2. **Action**: Intentar agregar línea de pedido
3. **Verification**: Verificar rechazo de operación

#### Resultado Esperado
- **Exception**: InvalidOperationException
- **Message**: "Cannot modify confirmed order"
- **State**: Pedido sin cambios

#### Resultado de Ejecución

| Fecha | Ambiente | Resultado | Ejecutado Por | Notas |
|-------|----------|-----------|---------------|-------|
| 2025-01-02 | TEST | ✅ PASS | QA Engineer | Business rule enforced |

---

## Test Execution Summary

### Estadísticas de Ejecución

| Métrica | Valor |
|---------|-------|
| **Total Test Cases** | 15 |
| **Executed** | 15 |
| **Passed** | 14 |
| **Failed** | 1 |
| **Success Rate** | 93.3% |
| **Coverage** | Order CRUD: 95% |

### Failed Test Cases

| Test ID | Título | Reason | Status | Assigned To |
|---------|--------|--------|--------|-------------|
| TC-002-08 | Cancel order with products in transit | Validation logic missing | Open | Backend Dev |

### Test Environment

```yaml
Environment: TEST
Database: PostgreSQL 15
API Version: v1.0
Test Framework: xUnit + TestContainers
Execution Date: 2025-01-02
Duration: 45 minutes
```

### Defects Found

#### DEF-002-001: Order Total Calculation
- **Severity**: Medium
- **Description**: Order total not recalculated when order line removed
- **Steps to Reproduce**: 
  1. Create order with 2 lines
  2. Remove one line
  3. Check total amount
- **Expected**: Total should decrease
- **Actual**: Total remains same
- **Status**: Fixed in build 1.0.1

### Test Data Cleanup

```sql
-- Cleanup script executed after test suite
DELETE FROM order_lines WHERE order_id IN (
    SELECT id FROM orders WHERE created_on_utc > '2025-01-02 08:00:00'
);

DELETE FROM orders WHERE created_on_utc > '2025-01-02 08:00:00';

-- Reset sequences if needed
SELECT setval('orders_id_seq', (SELECT MAX(id) FROM orders));
```

### Performance Metrics

| Operation | Average Time | P95 Time | Max Time |
|-----------|--------------|----------|----------|
| Create Order | 145ms | 280ms | 450ms |
| Confirm Order | 95ms | 180ms | 320ms |
| Get Order | 45ms | 85ms | 150ms |
| List Orders | 220ms | 450ms | 780ms |

---

*Last verified: 2025-01-02 - Commit: 4ef981b*