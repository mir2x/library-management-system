using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Commands.SuspendMember;

public record SuspendMemberCommand(Guid Id) : IRequest<Result>;
