# 📊 Diagrama MERM Completo - Sistema Conaprole Orders

Este diagrama representa el **Modelo Entidad-Relación** completo del sistema Conaprole Orders, incluyendo todas las entidades, relaciones, claves y atributos más relevantes del esquema de base de datos.

## 🗂️ Diagrama ERD Completo

```mermaid
erDiagram
    %% ========================================
    %% ENTIDADES PRINCIPALES
    %% ========================================
    
    USERS {
        uuid id PK
        varchar first_name "NOT NULL, MAX 200"
        varchar last_name "NOT NULL, MAX 200"
        varchar email UK "NOT NULL, MAX 400"
        varchar identity_id UK "NOT NULL - Keycloak ID"
        uuid distributor_id FK "NULLABLE"
        timestamptz created_at
        timestamptz updated_at
    }
    
    ROLES {
        int id PK "IDENTITY"
        varchar name UK "NOT NULL"
        varchar description
        timestamptz created_at
    }
    
    PERMISSIONS {
        int id PK "IDENTITY"
        varchar name UK "NOT NULL"
        varchar description
        timestamptz created_at
    }
    
    DISTRIBUTORS {
        uuid id PK
        varchar name "NOT NULL, MAX 100"
        varchar phone_number "NOT NULL, MAX 20"
        varchar address "NOT NULL, MAX 200"
        text supported_categories "JSON Array"
        timestamptz created_at
        timestamptz updated_at
    }
    
    POINTS_OF_SALE {
        uuid id PK
        varchar name "NOT NULL, MAX 100"
        varchar phone_number "NOT NULL, MAX 20"
        varchar address "NOT NULL, MAX 200"
        timestamptz created_at
        timestamptz updated_at
    }
    
    PRODUCTS {
        uuid id PK
        varchar name "NOT NULL"
        varchar description "NOT NULL"
        varchar external_product_id UK "NOT NULL - ERP ID"
        int category "NOT NULL - Enum"
        decimal unit_price_amount "NOT NULL, 18,2"
        varchar unit_price_currency "NOT NULL"
        timestamptz last_updated
    }
    
    ORDERS {
        uuid id PK
        uuid distributor_id FK "NOT NULL"
        uuid point_of_sale_id FK "NOT NULL"
        varchar status "NOT NULL - Enum"
        varchar delivery_address_street "NOT NULL"
        varchar delivery_address_city "NOT NULL"
        varchar delivery_address_zipcode "NOT NULL"
        decimal price_amount "18,2"
        varchar price_currency
        timestamptz created_on_utc "NOT NULL"
        timestamptz confirmed_on_utc "NULLABLE"
        timestamptz rejected_on_utc "NULLABLE"
        timestamptz delivery_on_utc "NULLABLE"
        timestamptz canceled_on_utc "NULLABLE"
        timestamptz delivered_on_utc "NULLABLE"
    }
    
    ORDER_LINES {
        uuid id PK
        uuid order_id FK "NOT NULL"
        uuid product_id FK "NOT NULL"
        int quantity "NOT NULL"
        decimal sub_total_amount "NOT NULL, 18,2"
        varchar sub_total_currency "NOT NULL"
        timestamptz created_on_utc "NOT NULL"
    }
    
    %% ========================================
    %% TABLAS DE UNIÓN / JUNCTION TABLES
    %% ========================================
    
    ROLE_USER {
        int roles_id FK "NOT NULL"
        uuid users_id FK "NOT NULL"
    }
    
    ROLE_PERMISSIONS {
        int permission_id FK "NOT NULL"
        int role_id FK "NOT NULL"
    }
    
    POINT_OF_SALE_DISTRIBUTORS {
        uuid id PK
        uuid point_of_sale_id FK "NOT NULL"
        uuid distributor_id FK "NOT NULL"
        varchar category "NOT NULL - Enum"
        timestamptz assigned_at "NOT NULL"
    }
    
    %% ========================================
    %% RELACIONES
    %% ========================================
    
    %% Autorización y Usuarios
    USERS ||--o{ ROLE_USER : "tiene roles"
    ROLES ||--o{ ROLE_USER : "asignado a usuarios"
    ROLES ||--o{ ROLE_PERMISSIONS : "tiene permisos"
    PERMISSIONS ||--o{ ROLE_PERMISSIONS : "otorgado a roles"
    
    %% Usuarios y Distribuidores
    DISTRIBUTORS ||--o{ USERS : "empleados"
    
    %% Distribuidores y Puntos de Venta (N:M)
    DISTRIBUTORS ||--o{ POINT_OF_SALE_DISTRIBUTORS : "sirve a"
    POINTS_OF_SALE ||--o{ POINT_OF_SALE_DISTRIBUTORS : "servido por"
    
    %% Pedidos
    DISTRIBUTORS ||--o{ ORDERS : "crea pedidos"
    POINTS_OF_SALE ||--o{ ORDERS : "recibe pedidos"
    ORDERS ||--o{ ORDER_LINES : "contiene líneas"
    PRODUCTS ||--o{ ORDER_LINES : "incluido en"
```

## 📋 Descripción de Entidades

### 🔑 Entidades Principales

#### USERS
- **Propósito**: Usuarios del sistema con autenticación via Keycloak
- **Claves**: `id` (PK), `email` (UK), `identity_id` (UK)
- **Relaciones**: Vinculado opcionalmente a un distribuidor

#### DISTRIBUTORS
- **Propósito**: Distribuidores que manejan productos y crean pedidos
- **Características**: Maneja categorías de productos soportadas como JSON
- **Relaciones**: Puede tener empleados (users) y servir múltiples puntos de venta

#### POINTS_OF_SALE
- **Propósito**: Puntos de venta que reciben pedidos
- **Relaciones**: Servidos por múltiples distribuidores según categoría

#### PRODUCTS
- **Propósito**: Catálogo de productos con pricing
- **Características**: Integración con ERP via `external_product_id`
- **Value Objects**: `UnitPrice` (Money)

#### ORDERS
- **Propósito**: Pedidos con ciclo de vida completo
- **Estados**: Created → Confirmed/Rejected → Delivered/Cancelled
- **Value Objects**: `Price` (Money), `DeliveryAddress` (Address)

#### ORDER_LINES
- **Propósito**: Líneas de pedido con productos y cantidades
- **Value Objects**: `SubTotal` (Money), `Quantity`

### 🔗 Tablas de Relación

#### ROLE_USER (N:M)
- **Propósito**: Asignación de roles a usuarios
- **Constraint**: Clave compuesta (`roles_id`, `users_id`)

#### ROLE_PERMISSIONS (N:M)
- **Propósito**: Permisos asignados a roles
- **Constraint**: Clave compuesta (`permission_id`, `role_id`)

#### POINT_OF_SALE_DISTRIBUTORS (N:M)
- **Propósito**: Relación distribuidor-punto de venta por categoría
- **Constraint**: Índice único (`point_of_sale_id`, `distributor_id`, `category`)

## 🎯 Características Técnicas

### Tipos de Datos
- **UUIDs**: Claves primarias para entidades de dominio
- **Integers**: Para roles, permisos y cantidades
- **Decimals**: Para valores monetarios (18,2)
- **Timestamps**: Con zona horaria para auditoría

### Constraints e Índices
- **Unique Constraints**: Emails, identity_ids, external_product_ids
- **Foreign Keys**: Con CASCADE delete para integridad referencial
- **Composite Indexes**: Para optimización de consultas frecuentes

### Value Objects
- **Money**: Amount + Currency para valores monetarios
- **Address**: Street + City + ZipCode para direcciones
- **Quantity**: Wrapper para cantidades con validación

## 🔄 Estados y Flujos

### Estados de Order
```
Created → Confirmed → Delivered
    ↓         ↓
Rejected  Cancelled
```

### Categorías de Productos
- LACTEOS
- CARNES
- BEBIDAS (deprecated)
- PANADERIA
- Y otros según enumeración

---

*Diagrama generado automáticamente a partir del esquema de base de datos Entity Framework Core*