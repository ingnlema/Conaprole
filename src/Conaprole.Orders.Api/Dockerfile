# Etapa base: imagen runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa build: compilación y publicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar el contenido de la carpeta "src" directamente a /src
COPY src/ .

# Restaurar dependencias del proyecto API
RUN dotnet restore "Conaprole.Orders.Api/Conaprole.Orders.Api.csproj"

# Establecer el directorio de trabajo al proyecto API y compilar
WORKDIR /src/Conaprole.Orders.Api
RUN dotnet build "Conaprole.Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación
RUN dotnet publish "Conaprole.Orders.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Etapa final: imagen runtime con la aplicación publicada
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Conaprole.Orders.Api.dll"]
