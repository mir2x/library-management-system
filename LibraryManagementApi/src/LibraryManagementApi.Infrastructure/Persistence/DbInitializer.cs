using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementApi.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    // Self-registration always assigns Member, and Branch/Member management both require an
    // existing Admin — without this there is no way to bootstrap the very first Admin account.
    // Dev-only: never run this against a Production configuration.
    public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = configuration["Seed:AdminEmail"]
            ?? throw new InvalidOperationException("Seed:AdminEmail configuration is missing.");
        var adminPassword = configuration["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword configuration is missing.");

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Default Admin",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
