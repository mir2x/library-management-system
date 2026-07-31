using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FullName) : IRequest<Result<AuthResponse>>;
