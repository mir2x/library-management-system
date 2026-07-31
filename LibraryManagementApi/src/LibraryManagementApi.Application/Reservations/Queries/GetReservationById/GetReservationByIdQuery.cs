using MediatR;

namespace LibraryManagementApi.Application.Reservations.Queries.GetReservationById;

public record GetReservationByIdQuery(Guid Id) : IRequest<ReservationDto?>;
