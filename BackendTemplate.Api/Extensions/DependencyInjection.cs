using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

namespace BackendTemplate.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HTTP Context
        services.AddHttpContextAccessor();

        // MVC Controllers
        services.AddControllers();

        // Endpoint Explorer para Swagger
        services.AddEndpointsApiExplorer();


        // =====================================================
        // CORS CONFIGURATION
        // =====================================================

        var corsSettings =
            configuration
                .GetSection("CorsSettings:AllowedOrigins")
                .Get<string[]>() 
            ?? Array.Empty<string>();


        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .WithOrigins(corsSettings)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });



        // =====================================================
        // RATE LIMITING
        // =====================================================

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown",

                        factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,

                                // Máximo de peticiones
                                PermitLimit = 100,

                                // Ventana de tiempo
                                Window = TimeSpan.FromMinutes(1)
                            }
                    )
                );


            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
        });



        // =====================================================
        // SWAGGER + JWT BEARER
        // =====================================================

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Backend Template API",
                    Version = "v1",
                    Description =
                        "Enterprise Backend Template API"
                });



            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",

                    Name = "Authorization",

                    In = ParameterLocation.Header,

                    Type = SecuritySchemeType.Http,

                    Scheme = "Bearer",

                    BearerFormat = "JWT"
                });



            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference =
                                new OpenApiReference
                                {
                                    Type =
                                        ReferenceType.SecurityScheme,

                                    Id = "Bearer"
                                }
                        },

                        Array.Empty<string>()
                    }
                });
        });



        // =====================================================
        // HEALTH CHECKS
        // =====================================================

        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection");


        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString!,
                name: "postgresql",
                timeout: TimeSpan.FromSeconds(5)
            );



        return services;
    }
}