using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagementApi.Infrastructure.Identity;

public class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<Result<AuthenticatedUser>> CreateUserAsync(string email, string password, string fullName, string role, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return Result<AuthenticatedUser>.Failure(createResult.Errors.Select(e => e.Description));
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            return Result<AuthenticatedUser>.Failure(roleResult.Errors.Select(e => e.Description));
        }

        return Result<AuthenticatedUser>.Success(new AuthenticatedUser(user.Id, user.Email!, user.FullName, [role]));
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new AuthenticatedUser(user.Id, user.Email!, user.FullName, roles.ToList());
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new AuthenticatedUser(user.Id, user.Email!, user.FullName, roles.ToList());
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        return await userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Don't reveal whether the account exists — same message as an invalid token.
            return Result.Failure(["Invalid or expired reset token."]);
        }

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(["Invalid or expired reset token."]);
    }
}
