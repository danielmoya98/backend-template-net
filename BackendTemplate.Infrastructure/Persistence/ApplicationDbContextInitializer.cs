using BackendTemplate.Domain.Entities;
using BackendTemplate.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql() || _context.Database.IsRelational())
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying pending EF Core migrations...");
                    await _context.Database.MigrateAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing or migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Default Roles
        var roles = new[] { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Administrator Account
        var administrator = new ApplicationUser
        {
            UserName = "admin@template.com",
            Email = "admin@template.com",
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            var result = await _userManager.CreateAsync(administrator, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(administrator, new[] { "Administrator", "User" });
            }
        }

        // 3. Seed Default Sample Data (Students)
        if (!await _context.Students.AnyAsync())
        {
            var sampleStudents = new List<Student>
            {
                new("Daniel", "Moya", "daniel.moya@example.com", "+1 555-0101", new DateTime(1998, 5, 15, 0, 0, 0, DateTimeKind.Utc)),
                new("Elena", "Reyes", "elena.reyes@example.com", "+1 555-0102", new DateTime(2000, 8, 22, 0, 0, 0, DateTimeKind.Utc)),
                new("Carlos", "Gomez", "carlos.gomez@example.com", "+1 555-0103", new DateTime(1999, 11, 3, 0, 0, 0, DateTimeKind.Utc))
            };

            await _context.Students.AddRangeAsync(sampleStudents);
            await _context.SaveChangesAsync();
        }
    }
}
