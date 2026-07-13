# Enterprise Backend Template (.NET 10)

Esta plantilla es un punto de partida robusto y altamente escalable para el desarrollo de APIs RESTful en .NET 10. Está diseñada bajo los principios de **Clean Architecture** y el patrón **CQRS** (Command Query Responsibility Segregation), priorizando la mantenibilidad, la testabilidad y el aislamiento de las reglas de negocio.

Ideal para construir desde sistemas de gestión institucional y plataformas logísticas complejas, hasta productos SaaS empresariales que requieran un alto rigor de calidad y auditoría.

---

## 🛠️ Stack Tecnológico y Características

El template ya incluye y configura las siguientes herramientas listas para producción:

* **Framework:** .NET 10 (Web API & Class Libraries)
* **Arquitectura:** Clean Architecture + Feature-Driven Design (CQRS)
* **Persistencia:** Entity Framework Core + PostgreSQL
* **Autenticación:** ASP.NET Core Identity + JWT (JSON Web Tokens)
* **Mediador:** MediatR (Manejo de Commands, Queries y Pipeline Behaviors)
* **Validaciones:** FluentValidation (Interceptadas automáticamente)
* **Observabilidad:** Serilog (Logs estructurados en consola y archivos rotativos)
* **Seguridad:** CORS configurado, Rate Limiting (anti-DDoS) y Security Headers (anti-XSS/Clickjacking)
* **Testing:** xUnit, Moq, FluentAssertions y Testcontainers (Pruebas Unitarias y de Integración)
* **Despliegue:** Dockerfile (Multi-stage) y HealthChecks integrados.

---

## 📦 Estructura del Proyecto

El código está estrictamente dividido en 4 capas para garantizar la regla de dependencia (de afuera hacia adentro):

1. **BackendTemplate.Domain:** El núcleo puro. Entidades, excepciones de negocio y el patrón `Result<T>`. No depende de nada.
2. **BackendTemplate.Application:** Casos de uso (Features), interfaces de contratos y validadores. Depende solo del Dominio.
3. **BackendTemplate.Infrastructure:** Detalles técnicos. Implementación de EF Core, Identity, generación de JWT y conexión a bases de datos.
4. **BackendTemplate.Api:** El mecanismo de entrega. Controladores, middlewares de excepciones globales, configuración de Swagger y seguridad.

---

## 🚀 Cómo usar este Template para Nuevos Proyectos

Para iniciar un nuevo desarrollo basado en esta arquitectura, sigue estos pasos:

1. Clona o copia esta carpeta base.
2. Renombra la solución (`.sln`), las carpetas y los namespaces (usando herramientas de refactorización de tu IDE) al nombre de tu nuevo proyecto (ej. `LogisticsSystem` o `SchoolManagement`).
3. Define tu cadena de conexión en `appsettings.Development.json` o mediante variables de entorno. Nota: Si usas bases de datos Serverless en la nube (como Neon), añade `SSL Mode=Require` al final del string.
4. Ejecuta las migraciones iniciales de EF Core para generar tus tablas de Identity:
   `dotnet ef database update --project BackendTemplate.Infrastructure --startup-project BackendTemplate.Api`
5. Ejecuta el proyecto con `dotnet run`.

---

## 🧩 Ejemplos Incluidos

El código fuente trae ejemplos funcionales para que sirvan de guía sobre cómo estructurar las funcionalidades:

* **Feature de Dominio (Student):** En `Application/Features/Students`, encontrarás el flujo completo de `CreateStudentCommand`, su `Handler` y su `Validator` con FluentValidation. Demuestra cómo manejar entidades que heredan de `AuditableEntity`.
* **Feature de Sistema (HealthCheck):** En `Application/Features/Health`, un ejemplo de `Query` simple que verifica el estado de la API, consumido a través del `HealthController`.
* **Respuestas Estandarizadas:** Todos los endpoints devuelven un formato JSON estandarizado (`ApiResponse<T>` o `ApiErrorResponse`).
* **Pruebas Unitarias:** En el proyecto `BackendTemplate.UnitTests`, un ejemplo de cómo testear el handler de `CreateStudent` utilizando `FluentAssertions`.

---

## 👨‍💻 Flujo de Trabajo para Desarrolladores (Cómo agregar nuevas Features)

Para mantener la arquitectura limpia, evita escribir lógica en los controladores. Cuando necesites agregar una nueva funcionalidad (ej. *Registrar una Nota* o *Despachar un Pedido*), sigue estrictamente este flujo:

**Paso 1: Dominio (Domain)**
Crea las entidades involucradas heredando de `BaseEntity` o `AuditableEntity`. Define aquí las reglas de negocio puras (ej. un método para calcular promedios o cambiar el estado de un envío).

**Paso 2: Aplicación (Application - Feature)**
Crea una nueva carpeta dentro de `Features/TuEntidad/Commands` o `Queries`.
Crea el modelo de entrada (`TuCommand` implementando `IRequest<Result<T>>`).
Crea el validador (`TuCommandValidator` heredando de `AbstractValidator`). Las reglas de FluentValidation se ejecutarán automáticamente.
Crea el manejador (`TuCommandHandler`). Aquí inyectas tu contexto de base de datos o repositorios para ejecutar la acción.

**Paso 3: Infraestructura (Infrastructure)**
Si la nueva entidad requiere configuración especial en la base de datos, agrégala al `ApplicationDbContext` o crea una clase de configuración (Fluent API). Crea una nueva migración con `dotnet ef migrations add NombreMigracion`.

**Paso 4: API (Presentación)**
Crea un nuevo endpoint en el controlador correspondiente. Haz que herede de `ApiControllerBase`.
Envía el comando al mediador usando `await Mediator.Send(new TuCommand(...))`.
Retorna la respuesta usando el método envoltorio `return HandleResult(resultado);`.

---

## 🔮 Extensiones Futuras (Escalabilidad)

Este template está preparado para evolucionar sin reescribir su base. Algunas extensiones que puedes integrar fácilmente en futuros proyectos:

* **Background Jobs / Tareas Programadas:** Si necesitas automatizar procesos pesados (como cierres de gestión automáticos a fin de año), puedes integrar **Hangfire** o **Quartz.NET** en la capa de Infraestructura y consumirlos desde la Aplicación.
* **Almacenamiento en la Nube:** Para manejar imágenes de perfil o documentos, crea una interfaz `IFileStorage` en `Application` e impleméntala en `Infrastructure` usando AWS S3, Cloudinary o Azure Blob Storage.
* **Caché Distribuido:** Para consultas de lectura intensiva, puedes implementar el patrón Decorator sobre tus Handlers de MediatR utilizando Redis (`IDistributedCache`).