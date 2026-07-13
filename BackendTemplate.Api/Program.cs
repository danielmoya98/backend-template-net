using BackendTemplate.Application.Extensions;
using BackendTemplate.Infrastructure.Extensions;
using BackendTemplate.Api.Extensions;
using BackendTemplate.Api.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando el servidor BackendTemplate...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    
    // Le pasamos la configuración para que pueda leer los dominios permitidos de CORS
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    
    // 🛡️ Middlewares de Seguridad
    app.UseMiddleware<GlobalExceptionMiddleware>();
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
    Log.CloseAndFlush();
}
public partial class Program { }
