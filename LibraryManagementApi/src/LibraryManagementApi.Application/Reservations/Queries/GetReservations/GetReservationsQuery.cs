using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Enums;
using MediatR;

namespace LibraryManagementApi.Application.Reservations.Queries.GetReservations;

public record GetReservationsQuery(
    Guid? MemberId,
    Guid? BookId,
    Guid? BranchId,
    ReservationStatus? Status,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<ReservationDto>>;
