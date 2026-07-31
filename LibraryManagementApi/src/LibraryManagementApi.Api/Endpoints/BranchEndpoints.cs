using LibraryManagementApi.Application.Branches;
using LibraryManagementApi.Application.Branches.Commands.CreateBranch;
using LibraryManagementApi.Application.Branches.Commands.DeleteBranch;
using LibraryManagementApi.Application.Branches.Commands.UpdateBranch;
using LibraryManagementApi.Application.Branches.Queries.GetBranchById;
using LibraryManagementApi.Application.Branches.Queries.GetBranches;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class BranchEndpoints
{
    public static IEndpointRouteBuilder MapBranchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/branches").WithTags("Branches").RequireAuthorization();

        group.MapGet("/", GetBranchesAsync)
            .WithName("GetBranches")
            .WithSummary("List branches (paginated, searchable by name/address).");

        group.MapGet("/{id:guid}", GetBranchByIdAsync)
            .WithName("GetBranchById")
            .WithSummary("Get a single branch by id.");

        group.MapPost("/", CreateBranchAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("CreateBranch")
            .WithSummary("Create a new branch.");

        group.MapPatch("/{id:guid}", UpdateBranchAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("UpdateBranch")
            .WithSummary("Partially update a branch. Omitted fields are left unchanged.");

        group.MapDelete("/{id:guid}", DeleteBranchAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("DeleteBranch")
            .WithSummary("Deactivate a branch (soft delete).");

        return app;
    }

    private static async Task<Ok<PaginatedList<BranchDto>>> GetBranchesAsync(
        [AsParameters] GetBranchesQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BranchDto>, NotFound>> GetBranchByIdAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBranchByIdQuery(id), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<BranchDto>, BadRequest<IEnumerable<string>>>> CreateBranchAsync(
        CreateBranchCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Created($"/api/branches/{result.Value!.Id}", result.Value)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<NoContent, BadRequest<IEnumerable<string>>>> UpdateBranchAsync(
        Guid id, UpdateBranchRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateBranchCommand(id, request.Name, request.Address, request.ContactNumber, request.Email);
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<NoContent> DeleteBranchAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteBranchCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    public record UpdateBranchRequest(string? Name, string? Address, string? ContactNumber, string? Email);
}
