# 📘 MANUAL TÉCNICO & GUÍA DE ARQUITECTURA
## Enterprise .NET Starter Kit (.NET 10 & C# 13)
**Versión:** 1.0.0 Enterprise Edition  
**Destinatarios:** Arquitectos de Software, Tech Leads, Desarrolladores Backend y Equipos de DevOps  
**Fecha de Publicación:** 2026  

---

## 📑 TABLA DE CONTENIDOS

1. [Resumen Ejecutivo & Visión General](#1-resumen-ejecutivo--visión-general)
2. [Arquitectura del Sistema (Clean Architecture & CQRS)](#2-arquitectura-del-sistema-clean-architecture--cqrs)
   - 2.1. Regla de Dependencia y Estructura de Proyectos
   - 2.2. Patrón CQRS con MediatR 14
   - 2.3. Pipeline Behaviors de MediatR
   - 2.4. Patrón Result & Error Fuertemente Tipado
   - 2.5. Auditoría Automática e Interceptores EF Core
3. [Seguridad, Autenticación & Identity](#3-seguridad-autenticación--identity)
   - 3.1. Flujo JWT y Ciclo de Vida de Refresh Tokens
   - 3.2. Protección contra Reutilización de Tokens (Anti-Replay Attack)
   - 3.3. Control de Acceso Basado en Roles (RBAC)
   - 3.4. Middlewares de Seguridad OWASP y Rate Limiting
   - 3.5. Manejo Centralizado de Excepciones RFC 7807 (ProblemDetails)
4. [Persistencia, Base de Datos & Resiliencia](#4-persistencia-base-de-datos--resiliencia)
   - 4.1. Entity Framework Core 10 & PostgreSQL 17
   - 4.2. Estrategia de Migraciones y Sembrador de Datos (Seeder)
   - 4.3. Resiliencia y Tolerancia a Fallos (Polly v8)
   - 4.4. Soporte Multi-Provider (PostgreSQL / InMemory)
5. [Guía Paso a Paso: Cómo Agregar un Nuevo Módulo / Feature](#5-guía-paso-a-paso-cómo-agregar-un-nuevo-módulo--feature)
   - 5.1. Paso 1: Modelado de Dominio (`Domain`)
   - 5.2. Paso 2: Casos de Uso y Validaciones (`Application`)
   - 5.3. Paso 3: Persistencia y Configuración Fluent API (`Infrastructure`)
   - 5.4. Paso 4: Controlador API y Endpoints (`Api`)
   - 5.5. Paso 5: Pruebas Unitarias y de Integración (`UnitTests` & `IntegrationTests`)
6. [Estrategia de Pruebas & Calidad de Código](#6-estrategia-de-pruebas--calidad-de-código)
   - 6.1. Pruebas de Arquitectura Automatizadas (`NetArchTest.Rules`)
   - 6.2. Pruebas Unitarias (`xUnit` + `Moq` + `FluentAssertions`)
   - 6.3. Pruebas de Integración con `WebApplicationFactory`
7. [Despliegue, Contenedores & Operaciones (DevOps)](#7-despliegue-contenedores--operaciones-devops)
   - 7.1. Variables de Entorno y `appsettings.json`
   - 7.2. Orquestación con `docker-compose.yml`
   - 7.3. Optimización y Seguridad en el `Dockerfile` (Non-Root User)
   - 7.4. Health Checks y Monitoreo de Infraestructura
   - 7.5. Logging Estructurado con Serilog
8. [Herramientas de Productividad (DX) & Referencia de API](#8-herramientas-de-productividad-dx--referencia-de-api)
   - 8.1. Documentación Interactiva Scalar (`/scalar/v1`)
   - 8.2. Cliente REST Integrado (`backend-requests.http`)
   - 8.3. Credenciales y Datos Semilla por Defecto

---

## 1. RESUMEN EJECUTIVO & VISIÓN GENERAL

El **Enterprise .NET Starter Kit** es una solución de ingeniería de software diseñada para acelerar el desarrollo de aplicaciones web de misión crítica, plataformas SaaS y APIs empresariales bajo **.NET 10** y **C# 13**.

### Objetivos Clave de la Solución:
* **Mantenibilidad a Largo Plazo:** Aislamiento total de las reglas de negocio respecto a frameworks externos y bases de datos.
* **Seguridad Empresarial:** Implementación estricta de las directrices OWASP Top 10, autenticación JWT con rotación criptográfica de tokens y mitigación anti-DDoS.
* **Escalabilidad Horizontal:** Diseñado como un monolito modular desacoplado, preparado para transicionar a microservicios sin reescritura de lógica de negocio.
* **Testabilidad 100%:** Arquitectura validada automáticamente por reglas de testing de arquitectura y pruebas de integración end-to-end.

---

## 2. ARQUITECTURA DEL SISTEMA (CLEAN ARCHITECTURE & CQRS)

La arquitectura sigue el patrón de **Clean Architecture** (Arquitectura Limpia) formulado por Robert C. Martin, combinado con el patrón **CQRS** (Command Query Responsibility Segregation) a través de **MediatR**.

```
+-------------------------------------------------------------+
|                     BackendTemplate.Api                     |
|           (Controllers, Middlewares, Scalar UI, Filters)    |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                 BackendTemplate.Application                 |
|      (Features, Commands, Queries, Validators, Behaviors)   |
+-------------------------------------------------------------+
        |                                             |
        | (Implementa Contratos)                      v
        |                               +-----------------------------+
        v                               |    BackendTemplate.Domain   |
+-----------------------------+         |  (Entities, Result<T>,      |
| BackendTemplate.Infrastructure        |   Error, Value Objects)     |
| (EF Core, Npgsql, JWT, Auth)| ------> +-----------------------------+
+-----------------------------+
```

### 2.1. Regla de Dependencia y Estructura de Proyectos

1. **`BackendTemplate.Domain` (Núcleo de Dominio):**
   * Contiene entidades puras, objetos de valor, tipos de error (`Error`, `ErrorType`), primitivas de respuesta (`Result`, `Result<T>`) e interfaces fundamentales (`ISoftDeletable`).
   * **Regla de Oro:** No tiene ninguna dependencia de proyectos externos ni de librerías de infraestructura.

2. **`BackendTemplate.Application` (Capa de Aplicación & Casos de Uso):**
   * Implementa los casos de uso del sistema organizados por *Vertical Slices* (Features).
   * Contiene interfaces de persistencia (`IApplicationDbContext`), contratos de servicios (`IIdentityService`, `ICurrentUserService`), validadores (`FluentValidation`) y pipeline behaviors de MediatR.
   * Depende únicamente de `BackendTemplate.Domain`.

3. **`BackendTemplate.Infrastructure` (Detalles Técnicos & Persistencia):**
   * Implementa el acceso a datos mediante Entity Framework Core 10, PostgreSQL (`Npgsql`), ASP.NET Core Identity y generación de tokens criptográficos.
   * Contiene interceptores de EF Core (`AuditableEntitySaveChangesInterceptor`) y el sembrador de base de datos (`ApplicationDbContextInitializer`).

4. **`BackendTemplate.Api` (Capa de Presentación & Entrega):**
   * Punto de entrada de la aplicación. Contiene los controladores REST (`ApiControllerBase`), middlewares de seguridad, manejador global de excepciones RFC 7807 (`CustomExceptionHandler`) y configuración de Scalar OpenAPI.

### 2.2. Patrón CQRS con MediatR 14

Se desacoplan las operaciones de escritura (**Commands**) de las de lectura (**Queries**):

* **Commands:** Modifican el estado del sistema. Retornan `Result` o `Result<T>`.
* **Queries:** Consultan datos sin modificar el estado. Están optimizadas con `.AsNoTracking()` para máxima velocidad de lectura.

### 2.3. Pipeline Behaviors de MediatR

Cada petición procesada por MediatR atraviesa automáticamente 4 capas de interceptación en el siguiente orden:

1. **`UnhandledExceptionBehavior`:** Captura cualquier excepción no controlada en el handler y la registra con contexto detallado en Serilog.
2. **`LoggingBehavior`:** Registra el inicio y fin de la ejecución del comando/query, incluyendo el identificador del usuario autenticado (`UserId`).
3. **`PerformanceBehavior`:** Monitorea el tiempo de ejecución y genera una alerta estructurada (`LogWarning`) si el caso de uso supera los **500 milisegundos**.
4. **`ValidationBehavior`:** Ejecuta automáticamente todos los validadores de FluentValidation registrados para el comando. Si existen fallos de validación, aborta la ejecución inmediatamente lanzando una `ValidationException` antes de tocar la base de datos.

### 2.4. Patrón Result & Error Fuertemente Tipado

Para evitar el uso de excepciones como mecanismo de control de flujo, todos los casos de uso retornan un contenedor `Result` o `Result<T>`:

```csharp
// Éxito
return Result<Guid>.Success(student.Id);

// Fallo con tipo semántico
return Result<StudentDto>.Failure(Error.NotFound("Students.NotFound", $"Student with Id '{id}' was not found."));
```

Tipos de error soportados (`ErrorType`):
* `Failure` (500 Internal Server Error)
* `Validation` (400 Bad Request con diccionario de campos)
* `NotFound` (404 Not Found)
* `Conflict` (409 Conflict)
* `Unauthorized` (401 Unauthorized)
* `Forbidden` (403 Forbidden)

### 2.5. Auditoría Automática e Interceptores EF Core

El interceptor `AuditableEntitySaveChangesInterceptor` intercepta todas las llamadas a `SaveChangesAsync()`:
* **Entidades `AuditableEntity`:** Asigna automáticamente `CreatedAt` y `CreatedBy` en inserción, y `LastModifiedAt` y `LastModifiedBy` en modificación, extrayendo el usuario actual desde `ICurrentUserService`.
* **Entidades `ISoftDeletable`:** Convierte operaciones de eliminación (`EntityState.Deleted`) en actualizaciones lógicas (`IsDeleted = true`, `DeletedAt = DateTime.UtcNow`, `DeletedBy = currentUser`), impidiendo la pérdida accidental de datos.

---

## 3. SEGURIDAD, AUTENTICACIÓN & IDENTITY

### 3.1. Flujo JWT y Ciclo de Vida de Refresh Tokens

La autenticación se basa en **JSON Web Tokens (JWT)** firmados con **HMAC-SHA256**:

1. **Inicio de Sesión (`POST /api/auth/login`):**
   * Valida credenciales contra `UserManager<ApplicationUser>`.
   * Genera un **AccessToken** de corta duración (configurable, ej. 60 minutos).
   * Genera un **RefreshToken** de 64 bytes criptográficamente aleatorios almacenado en la base de datos (`DbSet<RefreshToken>`).
   * Retorna el par de tokens y los roles del usuario.

2. **Rotación de Token (`POST /api/auth/refresh-token`):**
   * El cliente envía el `AccessToken` vencido y el `RefreshToken`.
   * El servidor valida la firma y emite un nuevo par de tokens.
   * El refresh token anterior se marca como usado (`IsUsed = true`) y se referencia al nuevo (`ReplacedByToken`).

### 3.2. Protección contra Reutilización de Tokens (Anti-Replay Attack)

Si un atacante intenta reutilizar un refresh token que ya fue marcado como `IsUsed = true`, el sistema detecta una brecha de seguridad inmediata, **revoca todos los tokens activos del usuario** y rechaza la petición con código `403 Forbidden`.

### 3.3. Control de Acceso Basado en Roles (RBAC)

Se configuran dos roles por defecto en el arranque:
* **`Administrator`:** Acceso total a endpoints administrativos y mutaciones.
* **`User`:** Acceso restringido a operaciones de lectura y perfil propio.

Uso en controladores:
```csharp
[Authorize(Roles = "Administrator")]
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id) => HandleResult(await Mediator.Send(new DeleteStudentCommand(id)));
```

### 3.4. Middlewares de Seguridad OWASP y Rate Limiting

* **`SecurityHeadersMiddleware`:** Inyecta en cada respuesta HTTP los encabezados:
  * `X-Content-Type-Options: nosniff` (previene ataques MIME sniffing).
  * `X-Frame-Options: DENY` (previene ataques de Clickjacking).
  * `Referrer-Policy: strict-origin-when-cross-origin`.
  * `Permissions-Policy: geolocation=(), camera=(), microphone=()`.
  * `Content-Security-Policy`: Configurado de forma segura para permitir Scalar UI y bloquear scripts no autorizados.
* **Rate Limiter (ASP.NET Core 10):** Limita las solicitudes a 100 peticiones por minuto por dirección IP, respondiendo `429 Too Many Requests` ante ráfagas maliciosas.

### 3.5. Manejo Centralizado de Excepciones RFC 7807 (ProblemDetails)

La clase `CustomExceptionHandler` implementa la interfaz nativa `IExceptionHandler` de .NET 10. Todas las respuestas de error siguen el estándar internacional **RFC 7807 `ProblemDetails`**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/students",
  "errors": {
    "Email": ["A valid email address is required."]
  }
}
```

---

## 4. PERSISTENCIA, BASE DE DATOS & RESILIENCIA

### 4.1. Entity Framework Core 10 & PostgreSQL 17

* Conexión optimizada mediante el driver de alto rendimiento `Npgsql.EntityFrameworkCore.PostgreSQL`.
* Configuraciones Fluent API aisladas por entidad en `Infrastructure/Persistence/Configurations/`.
* Filtro global de borrado lógico configurado automáticamente:
```csharp
builder.HasQueryFilter(s => !s.IsDeleted);
```

### 4.2. Estrategia de Migraciones y Sembrador de Datos (Seeder)

Al arrancar la API (cuando `Database:ApplyMigrationsOnStartup` es `true`), `ApplicationDbContextInitializer` ejecuta:
1. `Database.MigrateAsync()`: Aplica cualquier migración de esquema pendiente en PostgreSQL.
2. `SeedAsync()`: Siembra los roles del sistema (`Administrator`, `User`), la cuenta administrativa por defecto (`admin@template.com` / `Admin123!`) y datos semilla de prueba.

### 4.3. Resiliencia y Tolerancia a Fallos (Polly v8)

La conexión a base de datos cuenta con una política de reintentos con retroceso exponencial (*exponential backoff*):
```csharp
npgsqlOptions.EnableRetryOnFailure(
    maxRetryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(5),
    errorCodesToAdd: null);
```

### 4.4. Soporte Multi-Provider

El starter kit detecta automáticamente el proveedor según el connection string:
* **Producción / Staging:** PostgreSQL 17 (`Host=...;Database=...`).
* **Tests Automatizados / Offline Dev:** In-Memory Database ultra-rápido (`InMemory_...`).

---

## 5. GUÍA PASO A PASO: CÓMO AGREGAR UN NUEVO MÓDULO / FEATURE

Supongamos que deseas agregar un nuevo módulo de **Productos (`Product`)**. Sigue este procedimiento estandarizado:

### Paso 1: Modelado de Dominio (`BackendTemplate.Domain`)
Crea el archivo `BackendTemplate.Domain/Entities/Product.cs`:
```csharp
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Domain.Entities;

public class Product : AuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    private Product() { }

    public Product(string name, string sku, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Sku = sku;
        Price = price;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new DomainException("Price cannot be negative.");
        Price = newPrice;
    }

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
```

### Paso 2: Casos de Uso y Validaciones (`BackendTemplate.Application`)
1. Agrega el `DbSet<Product>` a `IApplicationDbContext.cs`:
```csharp
DbSet<Product> Products { get; }
```

2. Crea el Command, Validator y Handler en `Application/Features/Products/Commands/CreateProduct/CreateProductCommand.cs`:
```csharp
public record CreateProductCommand(string Name, string Sku, decimal Price) : IRequest<Result<Guid>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Sku).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Price).GreaterThan(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Sku, request.Price);
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(product.Id);
    }
}
```

### Paso 3: Persistencia y Configuración Fluent API (`BackendTemplate.Infrastructure`)
1. Agrega el `DbSet<Product>` a `ApplicationDbContext.cs`.
2. Crea `Infrastructure/Persistence/Configurations/ProductConfiguration.cs`:
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

### Paso 4: Controlador API (`BackendTemplate.Api`)
Crea `BackendTemplate.Api/Controllers/ProductsController.cs`:
```csharp
using BackendTemplate.Application.Features.Products.Commands.CreateProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

[Authorize]
public class ProductsController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}
```

---

## 6. ESTRATEGIA DE PRUEBAS & CALIDAD DE CÓDIGO

La solución cuenta con **29 pruebas automatizadas** que garantizan que el código cumple con las reglas de arquitectura y negocio:

```
[Tests] Total: 29 Superadas | 0 Fallos | 0 Omitidas (100% Passing)
├── BackendTemplate.UnitTests (24 pruebas)
│   ├── ArchitectureTests (6 pruebas con NetArchTest.Rules)
│   ├── AuthValidationTests (FluentValidation rules)
│   ├── AuthCommandHandlerTests (Login, Register, Refresh, Revoke, Password)
│   └── StudentCommandHandlerTests (Create, Update, Soft-Delete)
└── BackendTemplate.IntegrationTests (5 pruebas)
    ├── HealthIntegrationTests (Health checks /health y /api/health)
    └── AuthIntegrationTests (Registro real, Login fallido, Protección 401)
```

### 6.1. Ejecutar Pruebas
```bash
dotnet test
```

### 6.2. Reglas de Arquitectura Verificadas (`NetArchTest.Rules`)
* `Domain_Should_Not_HaveDependencyOn_OtherProjects`: El dominio no depende de ninguna otra capa.
* `Application_Should_Not_HaveDependencyOn_Infrastructure_Or_Api`: La aplicación no depende de infraestructura ni de controladores.
* `Infrastructure_Should_Not_HaveDependencyOn_Api`: La infraestructura no depende de la capa API.
* `Handlers_Should_Have_NameEndingWith_Handler`: Todos los handlers terminan con el sufijo `Handler`.
* `Validators_Should_Have_NameEndingWith_Validator`: Todos los validadores terminan con `Validator`.
* `Controllers_Should_Inherit_From_ApiControllerBase`: Todos los controladores heredan de `ApiControllerBase`.

---

## 7. DESPLIEGUE, CONTENEDORES & OPERACIONES (DEVOPS)

### 7.1. Variables de Entorno Clave

| Variable de Entorno | Propósito | Ejemplo / Valor Recomendado |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Production` o `Development` |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión PostgreSQL | `Host=postgres;Port=5432;Database=EnterpriseDb;Username=postgres;Password=SecretPassword!` |
| `JwtSettings__Secret` | Clave secreta HMAC-SHA256 | Cadena de al menos 64 caracteres seguros |
| `JwtSettings__Issuer` | Emisor del JWT | `https://api.tuempresa.com` |
| `JwtSettings__Audience` | Audiencia del JWT | `https://tuempresa.com` |
| `CorsSettings__AllowedOrigins__0` | Origen frontend permitido | `https://app.tuempresa.com` |

### 7.2. Despliegue con Docker Compose
```bash
# Iniciar servicios dependientes (PostgreSQL, Redis, Mailpit) y Backend API
docker compose up -d --build
```

### 7.3. Seguridad en el Contenedor Docker
* Compilación multi-etapa en base a imágenes oficiales de Microsoft .NET 10 SDK.
* Ejecución bajo usuario **no privilegiado** `USER app` (UID 1654), evitando privilegios de root dentro del contenedor.
* Health Check nativo de contenedor configurado con `curl` o probe HTTP.

---

## 8. HERRAMIENTAS DE PRODUCTIVIDAD (DX) & REFERENCIA DE API

### 8.1. Scalar API Reference
* **URL:** `http://localhost:5000/scalar/v1` (o `https://localhost:5001/scalar/v1`)
* Reemplazo moderno y elegante de Swagger UI.
* Permite probar endpoints directamente con soporte integrado para autorización **Bearer Token**.

### 8.2. Cliente REST Integrado (`backend-requests.http`)
Permite probar todos los endpoints desde Visual Studio o VS Code con la extensión *REST Client*. Al ejecutar el login, el token se captura automáticamente y se envía en las solicitudes autenticadas:

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@template.com",
  "password": "Admin123!"
}
```

### 8.3. Credenciales Semilla por Defecto
* **Admin Email:** `admin@template.com`
* **Admin Password:** `Admin123!`
* **Roles:** `Administrator`, `User`

---

*Enterprise .NET Starter Kit — Construido con los más altos estándares de la industria del software.*
