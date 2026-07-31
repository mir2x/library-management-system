using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Application.Reservations;

// Shared between ReturnBookCommandHandler and CancelReservationCommandHandler — both situations
// free up a physical copy and need the exact same decision: hand it to the next person waiting,
// or release it back to general availability.
public class ReservationAllocator(IApplicationDbContext context)
{
    public async Task ReleaseCopyAsync(Guid bookId, Guid branchId, CancellationToken cancellationToken)
    {
        var nextInLine = await context.Reservations
            .Where(r => r.BookId == bookId && r.BranchId == branchId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.ReservedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextInLine is not null)
        {
            // Hand the copy directly to the next reservation instead of releasing it — it must
            // not become generally available while someone is holding a place in line for it.
            nextInLine.MarkReady();
            return;
        }

        var inventory = await context.BookInventories
            .SingleOrDefaultAsync(i => i.BookId == bookId && i.BranchId == branchId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory record missing for a book that was previously in circulation.");

        inventory.Return();
    }
}
