using System.Security.Claims;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Members;
using LibraryManagementApi.Application.Members.Commands.CreateMember;
using LibraryManagementApi.Application.Members.Commands.DeleteMember;
using LibraryManagementApi.Application.Members.Commands.ReactivateMember;
using LibraryManagementApi.Application.Members.Commands.SuspendMember;
using LibraryManagementApi.Application.Members.Commands.UpdateMember;
using LibraryManagementApi.Application.Members.Queries.GetMemberById;
using LibraryManagementApi.Application.Members.Queries.GetMembers;
using LibraryManagementApi.Application.Members.Queries.GetMyMemberProfile;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Librarian));

        group.MapGet("/", GetMembersAsync)
            .WithName("GetMembers")
            .WithSummary("List members (paginated, searchable by name/email/membership number).");

        group.MapGet("/{id:guid}", GetMemberByIdAsync)
            .WithName("GetMemberById")
            .WithSummary("Get a single member by id.");

        group.MapPost("/", CreateMemberAsync)
            .WithName("CreateMember")
            .WithSummary("Register a walk-in member with no online account.");

        group.MapPatch("/{id:guid}", UpdateMemberAsync)
            .WithName("UpdateMember")
            .WithSummary("Partially update a member's profile. Omitted fields are left unchanged.");

        group.MapDelete("/{id:guid}", DeleteMemberAsync)
            .WithName("DeleteMember")
            .WithSummary("Deactivate a membership.");

        group.MapPost("/{id:guid}/suspend", SuspendMemberAsync)
            .WithName("SuspendMember")
            .WithSummary("Suspend a membership.");

        group.MapPost("/{id:guid}/reactivate", ReactivateMemberAsync)
            .WithName("ReactivateMember")
            .WithSummary("Reactivate a suspended membership.");

        // Any authenticated user can view their own profile — not restricted to Admin/Librarian.
        app.MapGet("/api/members/me", GetMyProfileAsync)
            .WithTags("Members")
            .RequireAuthorization()
            .WithName("GetMyMemberProfile")
            .WithSummary("Get the current user's own member profile.");

        return app;
    }

    private static async Task<Ok<PaginatedList<MemberDto>>> GetMembersAsync(
        [AsParameters] GetMembersQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<MemberDto>, NotFound>> GetMemberByIdAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMemberByIdQuery(id), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<MemberDto>, BadRequest<IEnumerable<string>>>> CreateMemberAsync(
        CreateMemberCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Ok(result.Value!)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<Results<NoContent, BadRequest<IEnumerable<string>>>> UpdateMemberAsync(
        Guid id, UpdateMemberRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateMemberCommand(id, request.FullName, request.Email, request.Phone, request.Address, request.HomeBranchId);
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<NoContent> DeleteMemberAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteMemberCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> SuspendMemberAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new SuspendMemberCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ReactivateMemberAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new ReactivateMemberCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<MemberDto>, NotFound>> GetMyProfileAsync(
        ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue("sub");
        if (userId is null)
        {
            return TypedResults.NotFound();
        }

        var result = await sender.Send(new GetMyMemberProfileQuery(userId), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    public record UpdateMemberRequest(string? FullName, string? Email, string? Phone, string? Address, Guid? HomeBranchId);
}
