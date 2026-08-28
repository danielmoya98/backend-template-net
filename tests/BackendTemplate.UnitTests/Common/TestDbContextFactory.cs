using BackendTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.UnitTests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("UnitTestDb_" + Guid.NewGuid())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
