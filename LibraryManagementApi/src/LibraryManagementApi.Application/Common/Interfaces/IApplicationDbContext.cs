using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Branch> Branches { get; }

    DbSet<Book> Books { get; }

    DbSet<BookInventory> BookInventories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
