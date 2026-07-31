using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(string UserId) : IRequest<Result<CurrentUserResponse>>;
