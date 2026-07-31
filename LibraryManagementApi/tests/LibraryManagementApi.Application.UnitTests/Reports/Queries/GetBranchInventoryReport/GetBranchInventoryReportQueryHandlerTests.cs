using LibraryManagementApi.Application.Reports.Queries.GetBranchInventoryReport;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetBranchInventoryReport;

public class GetBranchInventoryReportQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetBranchInventoryReportQueryHandler _handler;

    public GetBranchInventoryReportQueryHandlerTests()
    {
        _handler = new GetBranchInventoryReportQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ComputesTotalsAndUtilization()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var book2 = Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null);
        _context.Branches.Add(branch);
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var inventory1 = BookInventory.Create(book1.Id, branch.Id, 4);
        inventory1.Borrow();
        var inventory2 = BookInventory.Create(book2.Id, branch.Id, 6);
        _context.BookInventories.AddRange(inventory1, inventory2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchInventoryReportQuery(null), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(branch.Id, dto.BranchId);
        Assert.Equal(2, dto.TotalTitles);
        Assert.Equal(10, dto.TotalCopies);
        Assert.Equal(9, dto.AvailableCopies);
        Assert.Equal(10.0, dto.UtilizationPercentage);
    }

    [Fact]
    public async Task Handle_WithNoInventory_ReturnsZeroUtilization()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchInventoryReportQuery(null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(0, result[0].TotalTitles);
        Assert.Equal(0, result[0].TotalCopies);
        Assert.Equal(0, result[0].UtilizationPercentage);
    }

    [Fact]
    public async Task Handle_WithBranchIdFilter_ReturnsOnlyThatBranch()
    {
        var branch1 = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var branch2 = Branch.Create("Uptown Branch", "456 Side St", null, null);
        _context.Branches.AddRange(branch1, branch2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchInventoryReportQuery(branch1.Id), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(branch1.Id, result[0].BranchId);
    }

    [Fact]
    public async Task Handle_ExcludesInactiveBranches()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        branch.Deactivate();
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchInventoryReportQuery(null), CancellationToken.None);

        Assert.Empty(result);
    }
}
