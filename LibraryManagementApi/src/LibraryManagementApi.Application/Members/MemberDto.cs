using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.Members;

public record MemberDto(
    Guid Id,
    string MembershipNumber,
    string FullName,
    string Email,
    string? Phone,
    string? Address,
    Guid HomeBranchId,
    string HomeBranchName,
    MembershipStatus Status,
    DateTime JoinDateUtc);
