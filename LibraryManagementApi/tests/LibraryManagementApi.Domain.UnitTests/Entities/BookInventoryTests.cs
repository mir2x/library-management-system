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

    // Note: the "can't reduce total copies below the number currently borrowed" guard in
    // SetTotalCopies can't be exercised yet — Create/SetTotalCopies always keep AvailableCopies
    // in lockstep with TotalCopies, so no borrowed state is reachable through this entity's
    // current public API. That guard becomes testable once the Borrow & Return module adds a
    // method that decrements AvailableCopies independently of TotalCopies.
}
