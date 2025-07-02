# 📊 Diagrama de Clases - API Core Conaprole

## 📁 Archivos

- **Archivo principal**: `diagrama-clases.drawio`
- **Formato**: Draw.io XML editable
- **Ubicación**: `/docs/architecture/diagrama-clases.drawio`

## 🎯 Descripción

Este diagrama de clases UML representa **la arquitectura completa del dominio** de la API Core Conaprole, mostrando todas las entidades principales, sus relaciones, propiedades y métodos.

## 🏗️ Estructura del Diagrama

### **Paquetes Organizacionales**

El diagrama está organizado en los siguientes paquetes:

1. **Orders** - Agregado principal de pedidos
2. **Products** - Entidades de productos
3. **Distributors** - Agregado de distribuidores
4. **PointsOfSale** - Agregado de puntos de venta
5. **Users** - Agregado de usuarios, roles y permisos
6. **Shared** - Value Objects compartidos

### **Tipos de Entidades Representadas**

- 🔶 **Aggregate Roots** (Raíces de Agregado) - Color amarillo
- 🔷 **Entities** (Entidades) - Color amarillo
- 🔶 **Value Objects** (Objetos de Valor) - Color verde
- 🔴 **Enumerations** (Enumeraciones) - Color rojo
- ⚪ **Interfaces** - Color gris
- 🔵 **Abstract Classes** - Color azul

## 📋 Entidades Principales

### **Aggregate Roots**

- **Order** - Pedido principal con líneas de pedido
- **Distributor** - Distribuidor con categorías soportadas
- **PointOfSale** - Punto de venta con asignaciones de distribuidores
- **User** - Usuario con roles y permisos

### **Entities**

- **OrderLine** - Línea de pedido individual
- **Product** - Producto del catálogo
- **PointOfSaleDistributor** - Relación entre punto de venta y distribuidor
- **Role** - Rol de usuario
- **Permission** - Permiso específico
- **RolePermission** - Tabla de unión roles-permisos

### **Value Objects**

- **Money** - Dinero con cantidad y moneda
- **Address** - Dirección con ciudad, calle y código postal
- **Quantity** - Cantidad validada
- **Currency** - Moneda con código
- **OrderId** - Identificador de pedido
- **FirstName, LastName, Email** - Datos personales
- **Name, Description** - Descriptivos de productos
- **ExternalProductId** - ID externo de producto

### **Enumerations**

- **Status** - Estados del pedido (Created, Confirmed, Delivered, Canceled, Rejected)
- **Category** - Categorías de productos (CONGELADOS, LACTEOS, SUBPRODUCTOS)

## 🔗 Relaciones Principales

### **Composiciones (1:N)**

- Order ◆→ OrderLine (Un pedido contiene múltiples líneas)
- PointOfSale ◆→ PointOfSaleDistributor (Un POS tiene múltiples asignaciones)

### **Asociaciones (N:1)**

- Order →1 Distributor (Muchos pedidos a un distributor)
- Order →1 PointOfSale (Muchos pedidos de un POS)
- OrderLine →1 Product (Muchas líneas referencian un producto)
- User →0..1 Distributor (Usuario puede tener distribuidor)

### **Asociaciones Many-to-Many**

- User ↔ Role (Usuarios tienen múltiples roles)
- Role ↔ Permission (Roles tienen múltiples permisos)
- PointOfSale ↔ Distributor (A través de PointOfSaleDistributor)

### **Herencia**

- Order, OrderLine, Product, User, etc. → Entity (Todos heredan de Entity)
- Order, Distributor, PointOfSale, User → IAggregateRoot (Implementan interfaz)

## 🎨 Convenciones UML Utilizadas

- **◆** Composición (contiene, ciclo de vida dependiente)
- **→** Asociación/Navegabilidad
- **△** Herencia/Implementación
- **1, *, 0..1** Cardinalidades
- **+** Público
- **-** Privado
- **#** Protegido

## 🛠️ Cómo Usar el Archivo

1. **Abrir en Draw.io**:
   - Ir a [app.diagrams.net](https://app.diagrams.net)
   - File → Open → Seleccionar `diagrama-clases.drawio`

2. **Editar**:
   - Modificar clases, propiedades, métodos
   - Agregar nuevas relaciones
   - Reorganizar layout

3. **Exportar**:
   - File → Export as → PNG/PDF/SVG para presentaciones
   - File → Save as → XML para mantener editable

## ✅ Cumplimiento de Criterios

- [x] **Archivo editable** en formato DrawIO
- [x] **Entidades principales** del dominio representadas
- [x] **Relaciones correctas** con cardinalidades
- [x] **Símbolos UML estándar** utilizados
- [x] **Diseño organizado** con paquetes y colores
- [x] **Compatible** con Draw.io y editores similares

## 🔄 Mantenimiento

Este diagrama debe actualizarse cuando:

- Se agreguen nuevas entidades al dominio
- Se modifiquen relaciones existentes
- Se cambien propiedades o métodos importantes
- Se introduzcan nuevos agregados

---

*Generado automáticamente para API Core Conaprole*
