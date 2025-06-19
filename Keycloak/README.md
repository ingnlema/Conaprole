# Keycloak Deployment - Realm Conaprole (Persistente)

Este repositorio contiene todo lo necesario para construir y desplegar una imagen personalizada de Keycloak `v22.0.5` con importación inicial del realm `Conaprole` y **persistencia de datos** a través de una base de datos PostgreSQL externa. Esta guía está orientada a entornos de Azure utilizando Azure Container Apps.

---

## 📆 Finalidad

Este despliegue permite:

- Inicializar el Realm `Conaprole` con sus **clientes**, **roles** y **configuraciones base** necesarias para la integración con APIs.
- Garantizar **persistencia de datos** (usuarios, sesiones, configuraciones nuevas) mediante PostgreSQL.
- Ejecutar en Azure de forma segura y replicable.

> ⚠️ La importación del realm se realiza **una única vez al primer arranque** si el realm no existe en la base de datos. Es clave para que las APIs funcionen correctamente con los clientes configurados.

---

## 📆 Contenido

- `Dockerfile`: Imagen personalizada de Keycloak con configuración de hostname, proxy, realm y soporte para PostgreSQL.
- `conaprole-realm-export.json`: Export del realm con configuraciones iniciales.
- `README.md`: Esta guía.

---

## ✅ Requisitos previos

- Tener un **Azure Container Registry (ACR)** accesible.
- Contar con una **base de datos PostgreSQL en Azure** (crear una base llamada `keycloak`).
- Tener habilitado el servicio **Azure Container Apps**.

---

## 🚀 Despliegue paso a paso

### 1. Crear base de datos PostgreSQL (si no existe)

Conectarse al servidor PostgreSQL y ejecutar:

```sql
CREATE DATABASE keycloak
  WITH OWNER = <usuario>
       ENCODING = 'UTF8'
       CONNECTION LIMIT = -1;
```

> El usuario debe tener permisos de creación de tablas y esquemas.

---

### 2. Crear secretos en Azure (recomendado)

Desde el portal de Azure, en tu Container App:

1. Ir a `Settings` > `Secrets`.
2. Crear:

| Nombre           | Valor                 |
| ---------------- | --------------------- |
| `kc-db-username` | `usuario_postgres`    |
| `kc-db-password` | `contraseña_postgres` |
| `kc-admin-user`  | `admin`               |
| `kc-admin-pass`  | `admin`               |

---

### 3. Configurar variables de entorno

Ir a `Containers > Environment Variables` y configurar:

| Variable                  | Tipo             | Valor                                    |
| ------------------------- | ---------------- | ---------------------------------------- |
| `KEYCLOAK_ADMIN`          | Secret reference | `kc-admin-user`                          |
| `KEYCLOAK_ADMIN_PASSWORD` | Secret reference | `kc-admin-pass`                          |
| `KC_DB`                   | Manual entry     | `postgres`                               |
| `KC_DB_URL`               | Manual entry     | `jdbc:postgresql://<host>:5432/keycloak` |
| `KC_DB_USERNAME`          | Secret reference | `kc-db-username`                         |
| `KC_DB_PASSWORD`          | Secret reference | `kc-db-password`                         |

---

### 4. Dockerfile base persistente

```dockerfile
FROM quay.io/keycloak/keycloak:22.0.5

COPY conaprole-realm-export.json /opt/keycloak/data/import/realm.json

EXPOSE 8080

ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start",
  "--hostname=<HOSTNAME>",
  "--hostname-strict=false",
  "--hostname-strict-https=false",
  "--proxy=edge",
  "--spi-x-frame-options-enabled=false",
  "--import-realm"
]
```

> Reemplazar `<HOSTNAME>` por el hostname generado por Azure.

---

### 5. Build y push de la imagen

```bash
docker buildx build \
  --platform linux/amd64 \
  -t <registry>.azurecr.io/keycloak-persistent:latest \
  --push \
  .
```

> Evitá sobrescribir `keycloak:latest`. Esta versión persistente es segura para producción.

---

### 6. Crear o actualizar Container App

- Imagen: `<registry>.azurecr.io/keycloak-persistent:latest`
- Puerto: `8080`
- Ingress: `habilitado (público)`
- Tamaño sugerido: `0.5 vCPU / 1 GiB RAM`
- Command override: (vacío)
- Arguments override: (vacío)

---

## 🔒 Seguridad en producción

- Usar secretos para credenciales sensibles.
- Habilitar solo el hostname válido y HTTPS.
- Eliminar `--spi-x-frame-options-enabled=false` si no necesitás UI embebida.
- Configurar dominios personalizados y certificados TLS.
- Aplicar políticas CSP si tenés frontend propio.

---

## 📅 Verificación de persistencia

1. Crear un usuario o cliente desde la consola de admin.
2. Reiniciar el Container App (Stop + Start).
3. Confirmar que el dato sigue existiendo.

> Si los datos persisten: ✅ la integración con PostgreSQL está funcionando.

---

## 🔍 Recursos expuestos

- Realm: `https://<HOSTNAME>.azurecontainerapps.io/realms/Conaprole`
- Admin Console: `https://<HOSTNAME>.azurecontainerapps.io/admin/Conaprole/console/`

---

## ✉️ Contacto

Para consultas técnicas sobre este despliegue, contactar con el equipo de infraestructura o soporte.

---

Este README está diseñado para ser utilizado como referencia por otros equipos que deseen desplegar Keycloak en Azure con persistencia segura y configuración inicial automatizada.

