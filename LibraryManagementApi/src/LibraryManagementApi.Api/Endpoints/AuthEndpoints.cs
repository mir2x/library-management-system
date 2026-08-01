using System.Security.Claims;
using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.ForgotPassword;
using LibraryManagementApi.Application.Auth.Commands.Login;
using LibraryManagementApi.Application.Auth.Commands.Logout;
using LibraryManagementApi.Application.Auth.Commands.Refresh;
using LibraryManagementApi.Application.Auth.Commands.Register;
using LibraryManagementApi.Application.Auth.Commands.ResetPassword;
using LibraryManagementApi.Application.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;

namespace LibraryManagementApi.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new library member account.");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate and receive an access + refresh token.");

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("Exchange a valid refresh token for a new access + refresh token.");

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Revoke a refresh token, ending that session.");

        group.MapGet("/me", MeAsync)
            .RequireAuthorization()
            .WithName("Me")
            .WithSummary("Return the currently authenticated user.");

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName("ForgotPassword")
            .WithSummary("Send a password reset email if the account exists.");

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Complete a password reset using the token from the reset email.");

        return app;
    }

    private static async Task<Results<Ok<AuthResponse>, BadRequest<IEnumerable<string>>>> RegisterAsync(
        RegisterCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> LoginAsync(
        LoginCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Unauthorized();
    }

    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> RefreshAsync(
        RefreshCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Unauthorized();
    }

    private static async Task<NoContent> LogoutAsync(LogoutCommand command, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<CurrentUserResponse>, UnauthorizedHttpResult>> MeAsync(
        ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        // "sub" matches the literal claim type JwtTokenGenerator wrote into the token;
        // MapInboundClaims = false keeps ASP.NET Core from remapping it on the way in.
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Unauthorized();
    }

    private static async Task<NoContent> ForgotPasswordAsync(ForgotPasswordCommand command, ISender sender, CancellationToken cancellationToken)
    {
        // Always 204, regardless of whether the account exists — see the handler for why.
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, BadRequest<IEnumerable<string>>>> ResetPasswordAsync(
        ResetPasswordCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }
}
