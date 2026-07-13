# Etapa 1: Base de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa 2: Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solución y proyectos para restaurar caché
COPY ["backend-template.sln", "./"]
COPY ["BackendTemplate.Api/BackendTemplate.Api.csproj", "BackendTemplate.Api/"]
COPY ["BackendTemplate.Application/BackendTemplate.Application.csproj", "BackendTemplate.Application/"]
COPY ["BackendTemplate.Domain/BackendTemplate.Domain.csproj", "BackendTemplate.Domain/"]
COPY ["BackendTemplate.Infrastructure/BackendTemplate.Infrastructure.csproj", "BackendTemplate.Infrastructure/"]
COPY ["tests/BackendTemplate.UnitTests/BackendTemplate.UnitTests.csproj", "tests/BackendTemplate.UnitTests/"]
COPY ["tests/BackendTemplate.IntegrationTests/BackendTemplate.IntegrationTests.csproj", "tests/BackendTemplate.IntegrationTests/"]

RUN dotnet restore

# Copiar el resto del código
COPY . .
WORKDIR "/src/BackendTemplate.Api"
RUN dotnet build -c Release -o /app/build

# Etapa 3: Publicación
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackendTemplate.Api.dll"]
