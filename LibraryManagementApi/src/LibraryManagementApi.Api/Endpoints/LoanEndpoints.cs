using System.Security.Claims;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.Loans.Commands.BorrowBook;
using LibraryManagementApi.Application.Loans.Commands.ReturnBook;
using LibraryManagementApi.Application.Loans.Queries.GetLoanById;
using LibraryManagementApi.Application.Loans.Queries.GetLoans;
using LibraryManagementApi.Application.Loans.Queries.GetMyLoans;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/loans")
            .WithTags("Loans")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Librarian));

        group.MapGet("/", GetLoansAsync)
            .WithName("GetLoans")
            .WithSummary("List loans (paginated, filterable by member/book/branch/overdue).");

        group.MapGet("/{id:guid}", GetLoanByIdAsync)
            .WithName("GetLoanById")
            .WithSummary("Get a single loan by id.");

        group.MapPost("/", BorrowBookAsync)
            .WithName("BorrowBook")
            .WithSummary("Borrow a book on behalf of a member.");

        group.MapPost("/{id:guid}/return", ReturnBookAsync)
            .WithName("ReturnBook")
            .WithSummary("Return a borrowed book.");

        // Any authenticated user can view their own loan history — not restricted to Admin/Librarian.
        app.MapGet("/api/loans/me", GetMyLoansAsync)
            .WithTags("Loans")
            .RequireAuthorization()
            .WithName("GetMyLoans")
            .WithSummary("Get the current user's own loan history.");

        return app;
    }

    private static async Task<Ok<PaginatedList<LoanDto>>> GetLoansAsync(
        [AsParameters] GetLoansQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<LoanDto>, NotFound>> GetLoanByIdAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLoanByIdQuery(id), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<LoanDto>, BadRequest<IEnumerable<string>>>> BorrowBookAsync(
        BorrowBookCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<NoContent> ReturnBookAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new ReturnBookCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PaginatedList<LoanDto>>, UnauthorizedHttpResult>> GetMyLoansAsync(
        ClaimsPrincipal user, [AsParameters] MyLoansPagingRequest paging, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var result = await sender.Send(new GetMyLoansQuery(userId, paging.PageNumber, paging.PageSize), cancellationToken);

        return TypedResults.Ok(result);
    }

    public record MyLoansPagingRequest(int PageNumber = 1, int PageSize = 20);
}
