using LibraryManagementApi.Domain.Common;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.Entities;

public class Reservation : BaseAuditableEntity
{
    public const int MaxActiveReservationsPerMember = 5;

    private Reservation()
    {
    }

    private Reservation(Guid memberId, Guid bookId, Guid branchId)
    {
        MemberId = memberId;
        BookId = bookId;
        BranchId = branchId;
        ReservedAtUtc = DateTime.UtcNow;
        Status = ReservationStatus.Pending;
    }

    public Guid MemberId { get; private set; }

    public Guid BookId { get; private set; }

    public Guid BranchId { get; private set; }

    public DateTime ReservedAtUtc { get; private set; }

    public DateTime? ReadyAtUtc { get; private set; }

    public DateTime? FulfilledAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public ReservationStatus Status { get; private set; }

    public static Reservation Create(Guid memberId, Guid bookId, Guid branchId) => new(memberId, bookId, branchId);

    public void MarkReady()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new DomainException("Only a pending reservation can become ready for pickup.");
        }

        Status = ReservationStatus.Ready;
        ReadyAtUtc = DateTime.UtcNow;
    }

    public void MarkFulfilled()
    {
        if (Status != ReservationStatus.Ready)
        {
            throw new DomainException("Only a reservation that is ready for pickup can be fulfilled.");
        }

        Status = ReservationStatus.Fulfilled;
        FulfilledAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled)
        {
            throw new DomainException("This reservation can no longer be cancelled.");
        }

        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }
}
