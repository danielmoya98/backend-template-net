# Stage 1: Build & Restore with layer caching
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for optimal NuGet cache utilization
COPY ["BackendTemplate.Api/BackendTemplate.Api.csproj", "BackendTemplate.Api/"]
COPY ["BackendTemplate.Application/BackendTemplate.Application.csproj", "BackendTemplate.Application/"]
COPY ["BackendTemplate.Domain/BackendTemplate.Domain.csproj", "BackendTemplate.Domain/"]
COPY ["BackendTemplate.Infrastructure/BackendTemplate.Infrastructure.csproj", "BackendTemplate.Infrastructure/"]

RUN dotnet restore "BackendTemplate.Api/BackendTemplate.Api.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/BackendTemplate.Api"
RUN dotnet publish "BackendTemplate.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Stage 2: Runtime image (Slim, Non-Root user)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Switch to standard non-root user built into .NET container images
USER app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BackendTemplate.Api.dll"]
