using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Commands.CreateMyReservation;

public record CreateMyReservationCommand(string UserId, Guid BookId, Guid BranchId) : IRequest<Result<ReservationDto>>;
