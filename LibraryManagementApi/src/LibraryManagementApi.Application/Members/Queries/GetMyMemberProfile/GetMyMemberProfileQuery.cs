using MediatR;

namespace LibraryManagementApi.Application.Members.Queries.GetMyMemberProfile;

public record GetMyMemberProfileQuery(string UserId) : IRequest<MemberDto?>;
