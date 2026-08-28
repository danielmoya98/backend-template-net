using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BackendTemplate.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "InMemory_IntegrationTests");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");

        builder.ConfigureServices(services =>
        {
            // Clear external network health checks for isolated integration tests
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
            });
        });
    }
}
