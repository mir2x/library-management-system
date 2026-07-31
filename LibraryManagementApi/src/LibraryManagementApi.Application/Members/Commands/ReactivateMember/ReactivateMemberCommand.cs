using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Commands.ReactivateMember;

public record ReactivateMemberCommand(Guid Id) : IRequest<Result>;
