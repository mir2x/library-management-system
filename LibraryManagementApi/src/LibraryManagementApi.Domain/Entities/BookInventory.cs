using LibraryManagementApi.Domain.Common;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.Entities;

public class BookInventory : BaseAuditableEntity
{
    private BookInventory()
    {
    }

    private BookInventory(Guid bookId, Guid branchId, int totalCopies)
    {
        BookId = bookId;
        BranchId = branchId;
        TotalCopies = totalCopies;
        AvailableCopies = totalCopies;
    }

    public Guid BookId { get; private set; }

    public Guid BranchId { get; private set; }

    public int TotalCopies { get; private set; }

    public int AvailableCopies { get; private set; }

    public static BookInventory Create(Guid bookId, Guid branchId, int totalCopies)
    {
        if (totalCopies < 0)
        {
            throw new DomainException("Total copies cannot be negative.");
        }

        return new BookInventory(bookId, branchId, totalCopies);
    }

    public void SetTotalCopies(int newTotalCopies)
    {
        if (newTotalCopies < 0)
        {
            throw new DomainException("Total copies cannot be negative.");
        }

        var borrowedCopies = TotalCopies - AvailableCopies;
        if (newTotalCopies < borrowedCopies)
        {
            throw new DomainException($"Cannot reduce total copies below the {borrowedCopies} currently borrowed.");
        }

        AvailableCopies += newTotalCopies - TotalCopies;
        TotalCopies = newTotalCopies;
    }

    public void Borrow()
    {
        if (AvailableCopies <= 0)
        {
            throw new DomainException("No copies available to borrow.");
        }

        AvailableCopies--;
    }

    public void Return()
    {
        if (AvailableCopies >= TotalCopies)
        {
            throw new DomainException("Cannot return more copies than are accounted for.");
        }

        AvailableCopies++;
    }
}
