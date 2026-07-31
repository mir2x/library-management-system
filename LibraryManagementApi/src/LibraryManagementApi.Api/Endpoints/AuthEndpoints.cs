using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Login;
using LibraryManagementApi.Application.Auth.Commands.Refresh;
using LibraryManagementApi.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new library member account.");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate and receive an access + refresh token.");

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("Exchange a valid refresh token for a new access + refresh token.");

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
}
