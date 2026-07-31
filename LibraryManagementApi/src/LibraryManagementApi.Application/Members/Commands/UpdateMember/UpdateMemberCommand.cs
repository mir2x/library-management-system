using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand(Guid Id, string? FullName, string? Email, string? Phone, string? Address, Guid? HomeBranchId)
    : IRequest<Result>;
