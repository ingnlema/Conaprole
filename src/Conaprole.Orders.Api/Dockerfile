# Etapa base: imagen runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Usamos un usuario fijo; asegúrate de que "app" exista o ajusta según corresponda
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa build: compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar archivos de proyecto de cada sub-proyecto
COPY ["src/Conaprole.Orders.Api/Conaprole.Orders.Api.csproj", "src/Conaprole.Orders.Api/"]
COPY ["src/Conaprole.Orders.Application/Conaprole.Orders.Application.csproj", "src/Conaprole.Orders.Application/"]
COPY ["src/Conaprole.Orders.Domain/Conaprole.Orders.Domain.csproj", "src/Conaprole.Orders.Domain/"]
COPY ["src/Conaprole.Orders.Infrastructure/Conaprole.Orders.Infrastructure.csproj", "src/Conaprole.Orders.Infrastructure/"]

# Restaurar dependencias del proyecto API
RUN dotnet restore "src/Conaprole.Orders.Api/Conaprole.Orders.Api.csproj"

# Copiar todo el código restante
COPY . .

# Establecer el directorio de trabajo para el proyecto API
WORKDIR "/src/src/Conaprole.Orders.Api"
RUN dotnet build "Conaprole.Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Etapa publish: publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Conaprole.Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa final: imagen runtime con la aplicación publicada
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# (Opcional) Verifica que los archivos de configuración se incluyeron
# RUN ls -la /app

ENTRYPOINT ["dotnet", "Conaprole.Orders.Api.dll"]
