using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagementApi.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(m => m.MembershipNumber).IsRequired().HasMaxLength(20);
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(256);
        builder.Property(m => m.Phone).HasMaxLength(30);
        builder.Property(m => m.Address).HasMaxLength(500);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(m => m.MembershipNumber).IsUnique();

        // Enforced authoritatively in the application layer (case-insensitive, excludes
        // deactivated memberships); this index is a defense-in-depth safety net at the DB level.
        builder.HasIndex(m => m.Email).IsUnique().HasFilter($"\"Status\" <> '{MembershipStatus.Deactivated}'");

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(m => m.HomeBranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
