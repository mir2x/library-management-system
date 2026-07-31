using MediatR;

namespace LibraryManagementApi.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(Guid Id) : IRequest<MemberDto?>;
