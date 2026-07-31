using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;
