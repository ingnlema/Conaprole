# Keycloak Deployment - Realm Conaprole

Este repositorio contiene todo lo necesario para construir y desplegar una imagen personalizada de Keycloak `v22.0.5` con importación automática del realm `Conaprole` en Azure Container Apps.

---

## 📦 Contenido

- `Dockerfile`: Imagen personalizada de Keycloak con configuración de hostname, proxy y realm import.
- `conaprole-realm-export.json`: Export del realm que incluye usuarios, clientes y roles.
- `README.md`: Instrucciones actualizadas para despliegue exitoso en infraestructuras similares.

---

## 🚀 Pasos para desplegar

### 1. Build y push de la imagen

```bash
docker buildx build \
  --platform linux/amd64 \
  -t <registry>.azurecr.io/keycloak:latest \
  --push \
  .
```

> Reemplazar `<registry>` por el nombre de tu Azure Container Registry.

---

### 2. Crear Azure Container App

- Imagen: `<registry>.azurecr.io/keycloak:latest`
- Puerto expuesto: `8080`
- Ingress: habilitado público (HTTPS)
- Tamaño sugerido: `0.5 vCPU / 1 GiB RAM`
- Revisión continua: activada (opcional)

---

## ⚙️ Dockerfile base recomendado

```dockerfile
FROM quay.io/keycloak/keycloak:22.0.5

COPY conaprole-realm-export.json /opt/keycloak/data/import/realm.json

EXPOSE 8080

ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start",
  "--hostname=container-conaprole-keycloak.delightfulbay-f2b42d90.brazilsouth.azurecontainerapps.io",
  "--hostname-strict=false",
  "--hostname-strict-https=false",
  "--proxy=edge",
  "--spi-x-frame-options-enabled=false",
  "--import-realm"
]
```

---

## 🧩 Recomendaciones clave para funcionamiento correcto

- **HTTPS obligatorio**: asegurar que se acceda solo por HTTPS para evitar errores de “Mixed Content”.
- **Sin iframes**: Keycloak bloquea carga en iframes a menos que se configure `X-Frame-Options`. En este despliegue se desactiva por defecto para evitar errores.
- **Frontend URL del Realm**: ingresar manualmente en la consola de administración en:
  `Realm Settings → General → Frontend URL`

  ```text
  https://container-conaprole-keycloak.delightfulbay-f2b42d90.brazilsouth.azurecontainerapps.io
  ```

- **Acceso a consola de administración**:

  Si el acceso directo a:
  ```
  https://container-conaprole-keycloak.../admin/master/console/
  ```
  no funciona, usar esta URL generada por Keycloak como workaround:
  ```
  https://container-conaprole-keycloak.../realms/master/protocol/openid-connect/auth?... (con redirect_uri codificada)
  ```

---

## 🔐 Seguridad en producción

- Quitar `--spi-x-frame-options-enabled=false` si no se necesitan iframes.
- Utilizar certificados TLS válidos (no autofirmados).
- Configurar hostname propio y URIs válidas para clientes.
- Aplicar políticas CSP más estrictas si es necesario.

---

## ✅ Verificación

Una vez desplegado:

- Realm: `https://<app>.azurecontainerapps.io/realms/Conaprole`
- Admin Console: `https://<app>.azurecontainerapps.io/admin/Conaprole/console/`