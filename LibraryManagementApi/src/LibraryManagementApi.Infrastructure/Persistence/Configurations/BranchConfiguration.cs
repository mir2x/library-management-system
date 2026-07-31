using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementApi.Infrastructure.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(500);
        builder.Property(b => b.ContactNumber).HasMaxLength(30);
        builder.Property(b => b.Email).HasMaxLength(256);

        // Enforced authoritatively in the application layer (case-insensitive, active-only
        // check); this index is a case-sensitive defense-in-depth safety net at the DB level.
        builder.HasIndex(b => b.Name).IsUnique().HasFilter("\"IsActive\" = true");
    }
}
