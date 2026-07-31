using LibraryManagementApi.Application.Books;
using LibraryManagementApi.Application.Books.Commands.CreateBook;
using LibraryManagementApi.Application.Books.Commands.DeleteBook;
using LibraryManagementApi.Application.Books.Commands.SetBookInventory;
using LibraryManagementApi.Application.Books.Commands.UpdateBook;
using LibraryManagementApi.Application.Books.Queries.GetBookById;
using LibraryManagementApi.Application.Books.Queries.GetBooks;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementApi.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books").RequireAuthorization();

        group.MapGet("/", GetBooksAsync)
            .WithName("GetBooks")
            .WithSummary("List books (paginated, searchable by title/author/ISBN/genre).");

        group.MapGet("/{id:guid}", GetBookByIdAsync)
            .WithName("GetBookById")
            .WithSummary("Get a single book, including per-branch copy availability.");

        group.MapPost("/", CreateBookAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("CreateBook")
            .WithSummary("Add a book to the catalog.");

        group.MapPatch("/{id:guid}", UpdateBookAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("UpdateBook")
            .WithSummary("Partially update a book's catalog metadata. Omitted fields are left unchanged.");

        group.MapDelete("/{id:guid}", DeleteBookAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("DeleteBook")
            .WithSummary("Deactivate a book (soft delete).");

        group.MapPut("/{id:guid}/inventory/{branchId:guid}", SetBookInventoryAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("SetBookInventory")
            .WithSummary("Set the total copies of a book held at a branch.");

        return app;
    }

    private static async Task<Ok<PaginatedList<BookDto>>> GetBooksAsync(
        [AsParameters] GetBooksQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BookDetailDto>, NotFound>> GetBookByIdAsync(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookByIdQuery(id), cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<BookDto>, BadRequest<IEnumerable<string>>>> CreateBookAsync(
        CreateBookCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.Succeeded
            ? TypedResults.Created($"/api/books/{result.Value!.Id}", result.Value)
            : TypedResults.BadRequest(result.Errors.AsEnumerable());
    }

    private static async Task<NoContent> UpdateBookAsync(
        Guid id, UpdateBookRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(id, request.Title, request.Author, request.Genre, request.PublishedYear, request.Description);
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteBookAsync(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteBookCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Ok<BookInventoryDto>> SetBookInventoryAsync(
        Guid id, Guid branchId, SetBookInventoryRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new SetBookInventoryCommand(id, branchId, request.TotalCopies);
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public record UpdateBookRequest(string? Title, string? Author, string? Genre, int? PublishedYear, string? Description);

    public record SetBookInventoryRequest(int TotalCopies);
}
