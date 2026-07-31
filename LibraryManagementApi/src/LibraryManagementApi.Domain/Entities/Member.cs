using LibraryManagementApi.Domain.Common;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.Entities;

public class Member : BaseAuditableEntity
{
    private Member()
    {
    }

    private Member(string membershipNumber, string fullName, string email, string? phone, string? address, Guid homeBranchId, string? userId)
    {
        MembershipNumber = membershipNumber;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        HomeBranchId = homeBranchId;
        UserId = userId;
        Status = MembershipStatus.Active;
        JoinDateUtc = DateTime.UtcNow;
    }

    public string MembershipNumber { get; private set; } = string.Empty;

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Phone { get; private set; }

    public string? Address { get; private set; }

    public Guid HomeBranchId { get; private set; }

    // Null for members registered directly by staff (e.g. walk-in patrons with no online
    // account); set to the Identity user's id for members who self-registered.
    public string? UserId { get; private set; }

    public MembershipStatus Status { get; private set; }

    public DateTime JoinDateUtc { get; private set; }

    public static Member Create(string membershipNumber, string fullName, string email, string? phone, string? address, Guid homeBranchId, string? userId) =>
        new(membershipNumber, fullName, email, phone, address, homeBranchId, userId);

    public void Update(string fullName, string email, string? phone, string? address, Guid homeBranchId)
    {
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        HomeBranchId = homeBranchId;
    }

    public void Suspend()
    {
        if (Status == MembershipStatus.Deactivated)
        {
            throw new DomainException("Cannot suspend a deactivated membership.");
        }

        Status = MembershipStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status == MembershipStatus.Deactivated)
        {
            throw new DomainException("Cannot reactivate a deactivated membership.");
        }

        Status = MembershipStatus.Active;
    }

    public void Deactivate() => Status = MembershipStatus.Deactivated;
}
