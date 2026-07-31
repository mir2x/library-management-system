using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Branch> Branches { get; }

    DbSet<Book> Books { get; }

    DbSet<BookInventory> BookInventories { get; }

    DbSet<Member> Members { get; }

    DbSet<Loan> Loans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
