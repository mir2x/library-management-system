using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.UnitTests.TestSupport;

// A minimal, framework-agnostic stand-in for the real ApplicationDbContext (which lives in
// Infrastructure and additionally derives from IdentityDbContext). Backed by EF Core's InMemory
// provider so handler tests can exercise real DbSet/LINQ query behavior (Where, SingleOrDefaultAsync)
// without depending on Infrastructure or a real database. Provider-specific SQL behavior is covered
// separately by the Api.IntegrationTests project against real PostgreSQL.
public class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}

public static class TestApplicationDbContextFactory
{
    public static TestApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }
}
