using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace LibraryManagementApi.Api.IntegrationTests.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@library.test";
    public const string LibrarianEmail = "librarian@library.test";
    public const string SeedPassword = "Sup3rSecret!";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("library_management_test")
        .WithUsername("library_test")
        .WithPassword("library_test_password")
        .Build();

    public NoOpEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Program.cs reads ConnectionStrings:DefaultConnection and Jwt:Secret eagerly, as plain
        // statements before builder.Build() runs — well before WebApplicationFactory gets a
        // chance to splice in ConfigureAppConfiguration overrides at the Build() boundary. Env
        // vars are loaded by WebApplication.CreateBuilder() itself, at the very first line of
        // Program.cs, so they're the only override mechanism early enough for these two reads.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-key-do-not-use-in-production-1234567890");

        // Program.cs seeds Identity roles the moment the host starts (before app.Run()), which
        // happens as a side effect of first touching Services below. Migrations must already be
        // applied by then, so run them here against a standalone context, independent of the
        // app's own DI container and its host-startup sequence.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;
        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.MigrateAsync();
        }

        using var scope = Services.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        await identityService.CreateUserAsync(AdminEmail, SeedPassword, "Test Admin", Roles.Admin, CancellationToken.None);
        await identityService.CreateUserAsync(LibrarianEmail, SeedPassword, "Test Librarian", Roles.Librarian, CancellationToken.None);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        Dispose();
    }
}
