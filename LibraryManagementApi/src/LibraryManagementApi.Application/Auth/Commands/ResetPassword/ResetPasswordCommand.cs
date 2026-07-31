using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result>;
