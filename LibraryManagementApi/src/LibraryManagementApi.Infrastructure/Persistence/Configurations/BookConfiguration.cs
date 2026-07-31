using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementApi.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Author).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Isbn).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Genre).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(2000);

        // Enforced authoritatively in the application layer (active-only check); this index is
        // a defense-in-depth safety net at the DB level.
        builder.HasIndex(b => b.Isbn).IsUnique().HasFilter("\"IsActive\" = true");
    }
}
