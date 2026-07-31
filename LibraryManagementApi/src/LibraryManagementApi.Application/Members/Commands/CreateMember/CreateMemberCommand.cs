using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Commands.CreateMember;

public record CreateMemberCommand(string FullName, string Email, string? Phone, string? Address, Guid HomeBranchId)
    : IRequest<Result<MemberDto>>;
