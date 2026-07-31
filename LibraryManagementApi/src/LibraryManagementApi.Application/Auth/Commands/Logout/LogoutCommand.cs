using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
