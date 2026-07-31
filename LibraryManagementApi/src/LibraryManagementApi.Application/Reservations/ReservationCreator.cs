using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations;

// Shared between CreateReservationCommandHandler (staff-assisted, explicit member) and
// CreateMyReservationCommandHandler (self-service) — both resolve a Member differently but run
// the exact same eligibility rules and creation logic from there.
public class ReservationCreator(IApplicationDbContext context)
{
    public async Task<Result<ReservationDto>> CreateAsync(Member member, Guid bookId, Guid branchId, CancellationToken cancellationToken)
    {
        if (member.Status != MembershipStatus.Active)
        {
            return Result<ReservationDto>.Failure(["Member is not active and cannot make reservations."]);
        }

        var book = await context.Books.SingleOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException(nameof(Book), bookId);

        var branch = await context.Branches.SingleOrDefaultAsync(b => b.Id == branchId, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), branchId);

        var inventory = await context.BookInventories
            .SingleOrDefaultAsync(i => i.BookId == bookId && i.BranchId == branchId, cancellationToken);

        if (inventory is null)
        {
            return Result<ReservationDto>.Failure(["This book is not stocked at this branch."]);
        }

        if (inventory.AvailableCopies > 0)
        {
            return Result<ReservationDto>.Failure(["This book is currently available — borrow it directly instead of reserving."]);
        }

        var activeReservationCount = await context.Reservations
            .CountAsync(r => r.MemberId == member.Id && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), cancellationToken);

        if (activeReservationCount >= Reservation.MaxActiveReservationsPerMember)
        {
            return Result<ReservationDto>.Failure(
                [$"Member has reached the maximum of {Reservation.MaxActiveReservationsPerMember} active reservations."]);
        }

        var alreadyReserved = await context.Reservations
            .AnyAsync(
                r => r.MemberId == member.Id && r.BookId == bookId && r.BranchId == branchId
                    && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready),
                cancellationToken);

        if (alreadyReserved)
        {
            return Result<ReservationDto>.Failure(["Member already has an active reservation for this book at this branch."]);
        }

        var reservation = Reservation.Create(member.Id, bookId, branchId);
        context.Reservations.Add(reservation);
        await context.SaveChangesAsync(cancellationToken);

        var queuePosition = await context.Reservations
            .CountAsync(
                r => r.BookId == bookId && r.BranchId == branchId && r.Status == ReservationStatus.Pending
                    && r.ReservedAtUtc <= reservation.ReservedAtUtc,
                cancellationToken);

        return Result<ReservationDto>.Success(new ReservationDto(
            reservation.Id, reservation.MemberId, member.FullName, reservation.BookId, book.Title, reservation.BranchId, branch.Name,
            reservation.ReservedAtUtc, reservation.ReadyAtUtc, reservation.FulfilledAtUtc, reservation.CancelledAtUtc,
            reservation.Status, queuePosition));
    }
}
