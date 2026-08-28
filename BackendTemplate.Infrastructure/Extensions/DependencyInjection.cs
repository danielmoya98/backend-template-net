using System.Text;
using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Infrastructure.Identity;
using BackendTemplate.Infrastructure.Persistence;
using BackendTemplate.Infrastructure.Persistence.Interceptors;
using BackendTemplate.Infrastructure.Services;
using BackendTemplate.Infrastructure.Services.FileStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BackendTemplate.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // HTTP Client Factory
        services.AddHttpClient();

        // Interceptors
        services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();

        // Database Context with multi-provider flexibility (PostgreSQL by default, InMemory for fast testing/offline dev)
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase(string.IsNullOrWhiteSpace(connectionString) ? "BackendTemplateInMemoryDb" : connectionString);
            }
            else
            {
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);
                    });
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Identity Configuration with Roles & Token Providers
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddRoleManager<RoleManager<IdentityRole>>()
        .AddRoleValidator<RoleValidator<IdentityRole>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? "SuperSecretKeyForDevelopmentAndTemplatePurposesMustBeAtLeast32BytesLong!";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Common Services
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // File Storage Provider (Local / Cloudinary / Supabase)
        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";
        if (storageProvider.Equals("Cloudinary", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
        }
        else if (storageProvider.Equals("Supabase", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, SupabaseFileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        // Database Initializer / Seeder
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }
}
