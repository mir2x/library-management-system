using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.Loans;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Commands.FulfillReservation;

public record FulfillReservationCommand(Guid Id) : IRequest<Result<LoanDto>>;
