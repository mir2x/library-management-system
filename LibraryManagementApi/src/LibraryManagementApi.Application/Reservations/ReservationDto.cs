using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.Reservations;

public record ReservationDto(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid BookId,
    string BookTitle,
    Guid BranchId,
    string BranchName,
    DateTime ReservedAtUtc,
    DateTime? ReadyAtUtc,
    DateTime? FulfilledAtUtc,
    DateTime? CancelledAtUtc,
    ReservationStatus Status,
    int QueuePosition);
