using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateBookCommand, Result>
{
    public async Task<Result> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await context.Books.SingleOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), request.Id);

        book.Update(
            request.Title ?? book.Title,
            request.Author ?? book.Author,
            request.Genre ?? book.Genre,
            request.PublishedYear ?? book.PublishedYear,
            request.Description ?? book.Description);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
