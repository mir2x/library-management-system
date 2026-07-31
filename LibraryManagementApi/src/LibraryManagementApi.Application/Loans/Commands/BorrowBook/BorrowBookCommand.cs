using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Loans.Commands.BorrowBook;

public record BorrowBookCommand(Guid MemberId, Guid BookId, Guid BranchId) : IRequest<Result<LoanDto>>;
