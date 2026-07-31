using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await identityService.GetUserByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result<CurrentUserResponse>.Failure(["User not found."]);
        }

        return Result<CurrentUserResponse>.Success(new CurrentUserResponse(user.Id, user.Email, user.FullName, user.Roles));
    }
}
