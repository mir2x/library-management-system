using LibraryManagementApi.Application.Members;
using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    // Self-registration always assigns Member, and every staff-only endpoint requires an
    // existing Admin — without this there is no way to bootstrap the very first accounts, or
    // to exercise role-based UI/authorization locally. Dev-only: never run against Production.
    public static async Task SeedDevAccountsAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var adminEmail = configuration["Seed:AdminEmail"]
            ?? throw new InvalidOperationException("Seed:AdminEmail configuration is missing.");
        var adminPassword = configuration["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword configuration is missing.");
        var librarianEmail = configuration["Seed:LibrarianEmail"]
            ?? throw new InvalidOperationException("Seed:LibrarianEmail configuration is missing.");
        var librarianPassword = configuration["Seed:LibrarianPassword"]
            ?? throw new InvalidOperationException("Seed:LibrarianPassword configuration is missing.");
        var memberEmail = configuration["Seed:MemberEmail"]
            ?? throw new InvalidOperationException("Seed:MemberEmail configuration is missing.");
        var memberPassword = configuration["Seed:MemberPassword"]
            ?? throw new InvalidOperationException("Seed:MemberPassword configuration is missing.");

        await CreateUserIfMissingAsync(userManager, adminEmail, adminPassword, "Default Admin", Roles.Admin);
        await CreateUserIfMissingAsync(userManager, librarianEmail, librarianPassword, "Default Librarian", Roles.Librarian);
        var memberUser = await CreateUserIfMissingAsync(userManager, memberEmail, memberPassword, "Default Member", Roles.Member);

        if (memberUser is not null && !await context.Members.AnyAsync(m => m.UserId == memberUser.Id))
        {
            var branch = await context.Branches.FirstOrDefaultAsync();
            if (branch is null)
            {
                branch = Branch.Create("Main Branch", "123 Library Street", null, null);
                context.Branches.Add(branch);
                await context.SaveChangesAsync();
            }

            context.Members.Add(Member.Create(
                MembershipNumberGenerator.Generate(), memberUser.FullName, memberUser.Email!, phone: null, address: null, branch.Id, memberUser.Id));
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser?> CreateUserIfMissingAsync(
        UserManager<ApplicationUser> userManager, string email, string password, string fullName, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
