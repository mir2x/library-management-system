using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Loans.Commands.ReturnBook;

public record ReturnBookCommand(Guid Id) : IRequest<Result>;
