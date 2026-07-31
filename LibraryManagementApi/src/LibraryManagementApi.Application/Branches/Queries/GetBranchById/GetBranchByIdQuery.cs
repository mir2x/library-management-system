using MediatR;

namespace LibraryManagementApi.Application.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<BranchDto?>;
