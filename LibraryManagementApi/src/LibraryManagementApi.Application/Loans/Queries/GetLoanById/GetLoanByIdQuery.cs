using MediatR;

namespace LibraryManagementApi.Application.Loans.Queries.GetLoanById;

public record GetLoanByIdQuery(Guid Id) : IRequest<LoanDto?>;
