using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;
