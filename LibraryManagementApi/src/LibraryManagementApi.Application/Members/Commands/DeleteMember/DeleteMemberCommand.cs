using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest<Result>;
