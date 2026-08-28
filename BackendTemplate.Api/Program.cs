using BackendTemplate.Application.Extensions;
using BackendTemplate.Infrastructure.Extensions;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Api.Extensions;
using BackendTemplate.Api.Middlewares;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando Enterprise .NET Starter Kit API...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // Auto-migrate & seed database in development/configured environments
    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup", true))
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
            await initializer.InitializeAsync();
            await initializer.SeedAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not auto-migrate/seed database. Ensure the database server is running.");
        }
    }

    // Built-in RFC 7807 Exception Handler
    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        // Native OpenAPI + Modern Scalar API Reference UI (available at /scalar/v1)
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Enterprise Backend Template API")
                .WithTheme(ScalarTheme.Mars)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseHttpsRedirection();

    // Security Middlewares
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseCors("CorsPolicy");
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente debido a un error crítico");
}
finally
{
    // Don't close and flush during test runs to avoid killing the logger for parallel tests
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
    {
        Log.CloseAndFlush();
    }
}

public partial class Program { }
