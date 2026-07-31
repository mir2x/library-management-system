using System.Security.Claims;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.Reservations.Commands.CancelReservation;
using LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;
using LibraryManagementApi.Application.Reservations.Commands.CreateReservation;
using LibraryManagementApi.Application.Reservations.Commands.FulfillReservation;
using LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;
using LibraryManagementApi.Application.Reservations.Queries.GetReservationById;
using LibraryManagementApi.Application.Reservations.Queries.GetReservations;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithTags("Reservations")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Librarian));

        group.MapGet("/", GetReservationsAsync)
            .WithName("GetReservations")
            .WithSummary("List reservations (paginated, filterable by member/book/branch/status).");

        group.MapGet("/{id:guid}", GetReservationByIdAsync)
            .WithName("GetReservationById")
            .WithSummary("Get a single reservation by id.");

        group.MapPost("/", CreateReservationAsync)
            .WithName("CreateReservation")
            .WithSummary("Reserve a book on behalf of a member.");

        group.MapPost("/{id:guid}/fulfill", FulfillReservationAsync)
            .WithName("FulfillReservation")
            .WithSummary("Convert a ready-for-pickup reservation into a loan.");

        // Self-service routes: any authenticated user, not just Admin/Librarian.
        app.MapPost("/api/reservations/me", CreateMyReservationAsync)
            .WithTags("Reservations")
            .RequireAuthorization()
            .WithName("CreateMyReservation")
            .WithSummary("Reserve a book for the current user.");

        app.MapGet("/api/reservations/me", GetMyReservationsAsync)
            .WithTags("Reservations")
            .RequireAuthorization()
            .WithName("GetMyReservations")
            .WithSummary("Get the current user's own reservation history.");

        // Cancel is owner-or-staff, enforced inside the handler — not role-gated at the route.
        app.MapPost("/api/reservations/{id:guid}/cancel", CancelReservationAsync)
            .WithTags("Reservations")
            .RequireAuthorization()
            .WithName("CancelReservation")
            .WithSummary("Cancel a reservation (the owning member or staff).");

        return app;
    }

    private static async Task<Ok<PaginatedList<ReservationDto>>> GetReservationsAsync(
        [AsParameters] GetReservationsQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ReservationDto>, NotFound>> GetReservationByIdAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReservationByIdQuery(id), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<ReservationDto>, BadRequest<IEnumerable<string>>>> CreateReservationAsync(
        CreateReservationCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<Ok<LoanDto>, BadRequest<IEnumerable<string>>>> FulfillReservationAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new FulfillReservationCommand(id), cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<Ok<ReservationDto>, BadRequest<IEnumerable<string>>, UnauthorizedHttpResult>> CreateMyReservationAsync(
        ClaimsPrincipal user, CreateMyReservationRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var command = new CreateMyReservationCommand(userId, request.BookId, request.BranchId);
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<Ok<PaginatedList<ReservationDto>>, UnauthorizedHttpResult>> GetMyReservationsAsync(
        ClaimsPrincipal user, [AsParameters] MyReservationsPagingRequest paging, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var result = await sender.Send(new GetMyReservationsQuery(userId, paging.PageNumber, paging.PageSize), cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, BadRequest<IEnumerable<string>>, UnauthorizedHttpResult>> CancelReservationAsync(
        Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var isStaff = user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Librarian);
        var result = await sender.Send(new CancelReservationCommand(id, userId, isStaff), cancellationToken);

        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    public record CreateMyReservationRequest(Guid BookId, Guid BranchId);

    public record MyReservationsPagingRequest(int PageNumber = 1, int PageSize = 20);
}
