using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<Result>;
