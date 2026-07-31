using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid Id, string? Name, string? Address, string? ContactNumber, string? Email) : IRequest<Result>;
