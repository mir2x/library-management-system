using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(Guid Id, string CallerUserId, bool CallerIsStaff) : IRequest<Result>;
