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
  "--hostname=<HOSTNAME>",
  "--hostname-strict=false",
  "--hostname-strict-https=false",
  "--proxy=edge",
  "--spi-x-frame-options-enabled=false",
  "--import-realm"
]
```

> 🔁 Reemplazar `<HOSTNAME>` por el hostname que genere Azure para tu Container App.

---

## 🔐 Variables de entorno recomendadas

En tu Azure Container App, configurar estas variables de entorno desde Azure portal:

| Nombre                          | Tipo              | Valor sugerido         |
|---------------------------------|-------------------|------------------------|
| `Keycloak__AdminClientSecret`   | Referencia secreto| `kc-admin-client`      |
| `Keycloak__AuthClientSecret`    | Referencia secreto| `kc-auth-client`       |

Estas claves serán accedidas automáticamente desde tu `appsettings.Staging.json`.

---

## 🧩 Recomendaciones clave

- **HTTPS obligatorio**: asegurar que se acceda por HTTPS para evitar errores de “Mixed Content”.
- **Frontend URL del Realm**: luego del despliegue, editar manualmente en:

  ```
  Realm Settings → General → Frontend URL
  ```

  Usar el dominio generado por Azure como:
  ```
  https://<HOSTNAME>.azurecontainerapps.io
  ```

---

## 🔐 Seguridad en producción

- **Rotar secretos**: evitar usar secretos estáticos exportados en el JSON. Se recomienda rotarlos y gestionarlos vía Azure Key Vault o configuraciones de entorno.
- **Eliminar `--spi-x-frame-options-enabled=false`** si no se necesita UI embebida.
- **Agregar políticas CSP** si la app lo requiere.
- **Usar dominios personalizados y certificados TLS válidos.**

---

## ✅ Verificación

Una vez desplegado:

- Realm: `https://<HOSTNAME>.azurecontainerapps.io/realms/Conaprole`
- Admin Console: `https://<HOSTNAME>.azurecontainerapps.io/admin/Conaprole/console/`