using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(Guid MemberId, Guid BookId, Guid BranchId) : IRequest<Result<ReservationDto>>;
