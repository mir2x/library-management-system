using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Domain.UnitTests.Entities;

public class BookInventoryTests
{
    [Fact]
    public void Create_WithNonNegativeTotalCopies_SetsAvailableCopiesEqualToTotal()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        Assert.Equal(5, inventory.TotalCopies);
        Assert.Equal(5, inventory.AvailableCopies);
    }

    [Fact]
    public void Create_WithNegativeTotalCopies_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), -1));
    }

    [Fact]
    public void SetTotalCopies_Increasing_IncreasesAvailableCopiesByTheSameAmount()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        inventory.SetTotalCopies(8);

        Assert.Equal(8, inventory.TotalCopies);
        Assert.Equal(8, inventory.AvailableCopies);
    }

    [Fact]
    public void SetTotalCopies_Decreasing_DecreasesAvailableCopiesByTheSameAmount()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        inventory.SetTotalCopies(2);

        Assert.Equal(2, inventory.TotalCopies);
        Assert.Equal(2, inventory.AvailableCopies);
    }

    [Fact]
    public void SetTotalCopies_ToZero_Succeeds()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        inventory.SetTotalCopies(0);

        Assert.Equal(0, inventory.TotalCopies);
        Assert.Equal(0, inventory.AvailableCopies);
    }

    [Fact]
    public void SetTotalCopies_Negative_ThrowsDomainException()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        Assert.Throws<DomainException>(() => inventory.SetTotalCopies(-1));
    }

    [Fact]
    public void Borrow_WithAvailableCopies_DecrementsAvailableCopiesOnly()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        inventory.Borrow();

        Assert.Equal(5, inventory.TotalCopies);
        Assert.Equal(4, inventory.AvailableCopies);
    }

    [Fact]
    public void Borrow_WithNoAvailableCopies_ThrowsDomainException()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 1);
        inventory.Borrow();

        Assert.Throws<DomainException>(inventory.Borrow);
    }

    [Fact]
    public void Return_AfterABorrow_IncrementsAvailableCopiesBackToTotal()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);
        inventory.Borrow();

        inventory.Return();

        Assert.Equal(5, inventory.AvailableCopies);
    }

    [Fact]
    public void Return_WithNothingBorrowed_ThrowsDomainException()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        Assert.Throws<DomainException>(inventory.Return);
    }

    [Fact]
    public void SetTotalCopies_BelowCurrentlyBorrowedCount_ThrowsDomainException()
    {
        // Now that Borrow() can decrement AvailableCopies independently of TotalCopies, this
        // guard (previously unreachable — see git history) is finally exercisable.
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);
        inventory.Borrow();
        inventory.Borrow(); // 2 borrowed, 3 available

        Assert.Throws<DomainException>(() => inventory.SetTotalCopies(1));
    }

    [Fact]
    public void SetTotalCopies_AtOrAboveCurrentlyBorrowedCount_Succeeds()
    {
        var inventory = BookInventory.Create(Guid.NewGuid(), Guid.NewGuid(), 5);
        inventory.Borrow();
        inventory.Borrow(); // 2 borrowed, 3 available

        inventory.SetTotalCopies(2);

        Assert.Equal(2, inventory.TotalCopies);
        Assert.Equal(0, inventory.AvailableCopies);
    }
}
