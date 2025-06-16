# Conaprole Orders API - Logs

## Configuración de Logs

La aplicación utiliza **Serilog** para el registro de logs con las siguientes características:

### Ubicación de los Logs

Los logs se almacenan en el directorio `logs/` en la raíz del proyecto API con el siguiente formato:

```
logs/log-YYYYMMDD.txt
```

Donde YYYYMMDD representa la fecha (año, mes, día). Los archivos se rotan diariamente de forma automática.

### Ejemplos de Archivos de Log

- `logs/log-20241215.txt` - Logs del 15 de diciembre de 2024
- `logs/log-20241216.txt` - Logs del 16 de diciembre de 2024

### Nivel de Logs

- **Nivel mínimo**: Information
- **Salidas**: Consola + Archivo
- Se registran todos los errores, excepciones y eventos informativos

### Contenido de los Logs

Los logs incluyen:

- **Información general**: Inicio y procesamiento de comandos MediatR
- **Errores de aplicación**: Excepciones capturadas por el middleware
- **Eventos de negocio**: Operaciones importantes de la API
- **Formato**: Timestamp ISO 8601 + Nivel + Mensaje

### Ejemplo de Contenido

```
2024-12-15 10:30:15.123 +00:00 [INF] Starting the Conaprole Orders API
2024-12-15 10:30:16.456 +00:00 [INF] Executing command CreateOrderCommand
2024-12-15 10:30:16.789 +00:00 [INF] Command CreateOrderCommand processed successfully
2024-12-15 10:31:02.345 +00:00 [ERR] Exception occurred: Validation failed for field Email
```

### Acceso en Diferentes Ambientes

#### Desarrollo Local
Los logs se encuentran en: `src/Conaprole.Orders.Api/logs/`

#### Producción/Docker
Los logs están dentro del contenedor. Para acceder:

```bash
# Ver logs en tiempo real
docker logs conaprole-orders-api

# Copiar archivos de log del contenedor
docker cp conaprole-orders-api:/app/logs ./local-logs/

# Acceder al contenedor para ver logs
docker exec -it conaprole-orders-api ls -la /app/logs/
```

### Consulta de Logs

Para buscar información específica en los logs:

```bash
# Ver todos los errores del día actual
grep "ERR" logs/log-$(date +%Y%m%d).txt

# Ver logs de un comando específico
grep "CreateOrderCommand" logs/log-*.txt

# Ver los últimos logs en tiempo real (desarrollo)
tail -f logs/log-$(date +%Y%m%d).txt
```

### Configuración

La configuración de Serilog se encuentra en `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
```

### Notas

- Los archivos de logs se crean automáticamente cuando la aplicación se inicia
- No es necesario crear manualmente el directorio `logs/`
- Los logs históricos se mantienen hasta que se eliminen manualmente
- Para ambientes de producción, considerar implementar rotación automática o limpieza de logs antiguos