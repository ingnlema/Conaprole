FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Conaprole.Orders/Conaprole.Orders.csproj", "Conaprole.Orders/"]
RUN dotnet restore "Conaprole.Orders/Conaprole.Orders.csproj"
COPY . .
WORKDIR "/src/Conaprole.Orders"
RUN dotnet build "Conaprole.Orders.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Conaprole.Orders.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conaprole.Orders.dll"]
