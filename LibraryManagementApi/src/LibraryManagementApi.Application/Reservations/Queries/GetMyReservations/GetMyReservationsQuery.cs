using LibraryManagementApi.Application.Common.Models;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;

public record GetMyReservationsQuery(string UserId, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<ReservationDto>>;
