using BackendTemplate.Api.Middlewares;
using BackendTemplate.Api.OpenApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

namespace BackendTemplate.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HTTP Context Accessor
        services.AddHttpContextAccessor();

        // MVC Controllers
        services.AddControllers();

        // Modern ProblemDetails & Exception Handling (.NET 8/10)
        services.AddProblemDetails();
        services.AddExceptionHandler<CustomExceptionHandler>();

        // Native OpenAPI configuration with Bearer Transformer for Scalar
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        // =====================================================
        // CORS CONFIGURATION
        // =====================================================
        var corsSettings = configuration
            .GetSection("CorsSettings:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                if (corsSettings.Length > 0)
                {
                    policy.WithOrigins(corsSettings)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        // =====================================================
        // RATE LIMITING
        // =====================================================
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }
                )
            );

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // =====================================================
        // HEALTH CHECKS
        // =====================================================
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: "postgresql",
                    timeout: TimeSpan.FromSeconds(5)
                );
        }
        else
        {
            services.AddHealthChecks();
        }

        return services;
    }
}