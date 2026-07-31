using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Branches.Commands.CreateBranch;

public record CreateBranchCommand(string Name, string Address, string? ContactNumber, string? Email) : IRequest<Result<BranchDto>>;
