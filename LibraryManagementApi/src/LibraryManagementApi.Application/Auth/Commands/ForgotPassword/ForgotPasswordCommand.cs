using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
