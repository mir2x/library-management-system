using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.Property(rt => rt.UserId).IsRequired();
            entity.HasIndex(rt => rt.UserId);
        });
    }
}
