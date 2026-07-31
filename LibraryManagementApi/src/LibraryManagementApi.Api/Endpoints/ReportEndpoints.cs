using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Reports;
using LibraryManagementApi.Application.Reports.Queries.GetBranchInventoryReport;
using LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;
using LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;
using LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;
using LibraryManagementApi.Application.Reports.Queries.GetReservationQueueSummaryReport;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Librarian));

        group.MapGet("/overdue-loans", GetOverdueLoansAsync)
            .WithName("GetOverdueLoansReport")
            .WithSummary("List currently overdue loans (paginated, filterable by branch).");

        group.MapGet("/most-borrowed-books", GetMostBorrowedBooksAsync)
            .WithName("GetMostBorrowedBooksReport")
            .WithSummary("Top borrowed books, optionally filtered by branch and date range.");

        group.MapGet("/branch-inventory", GetBranchInventoryAsync)
            .WithName("GetBranchInventoryReport")
            .WithSummary("Per-branch inventory totals and utilization.");

        group.MapGet("/member-activity", GetMemberActivityAsync)
            .WithName("GetMemberActivityReport")
            .WithSummary("Per-member loan and reservation activity (paginated, filterable by branch).");

        group.MapGet("/reservation-queues", GetReservationQueueSummaryAsync)
            .WithName("GetReservationQueueSummaryReport")
            .WithSummary("Pending/ready reservation queue length per book and branch.");

        return app;
    }

    private static async Task<Ok<PaginatedList<OverdueLoanDto>>> GetOverdueLoansAsync(
        [AsParameters] GetOverdueLoansReportQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<MostBorrowedBookDto>>> GetMostBorrowedBooksAsync(
        [AsParameters] GetMostBorrowedBooksReportQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<BranchInventorySummaryDto>>> GetBranchInventoryAsync(
        [AsParameters] GetBranchInventoryReportQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PaginatedList<MemberActivityDto>>> GetMemberActivityAsync(
        [AsParameters] GetMemberActivityReportQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<List<ReservationQueueSummaryDto>>> GetReservationQueueSummaryAsync(
        [AsParameters] GetReservationQueueSummaryReportQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
