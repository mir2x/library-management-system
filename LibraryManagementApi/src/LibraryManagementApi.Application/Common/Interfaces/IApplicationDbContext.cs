using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
