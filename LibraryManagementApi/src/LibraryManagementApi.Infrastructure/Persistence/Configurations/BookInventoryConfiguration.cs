using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementApi.Infrastructure.Persistence.Configurations;

public class BookInventoryConfiguration : IEntityTypeConfiguration<BookInventory>
{
    public void Configure(EntityTypeBuilder<BookInventory> builder)
    {
        builder.HasIndex(i => new { i.BookId, i.BranchId }).IsUnique();

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(i => i.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
