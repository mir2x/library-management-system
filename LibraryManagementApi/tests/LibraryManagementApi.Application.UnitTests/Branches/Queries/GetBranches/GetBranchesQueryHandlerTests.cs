using LibraryManagementApi.Application.Branches.Queries.GetBranches;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Branches.Queries.GetBranches;

public class GetBranchesQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetBranchesQueryHandler _handler;

    public GetBranchesQueryHandlerTests()
    {
        _handler = new GetBranchesQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ExcludesDeactivatedBranches()
    {
        var active = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var inactive = Branch.Create("Old Branch", "456 Elm St", null, null);
        inactive.Deactivate();
        _context.Branches.AddRange(active, inactive);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchesQuery(null), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Downtown Branch", result.Items[0].Name);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByNameOrAddress()
    {
        _context.Branches.AddRange(
            Branch.Create("Downtown Branch", "123 Main St", null, null),
            Branch.Create("Uptown Branch", "456 Elm St", null, null),
            Branch.Create("Riverside", "789 Downtown Ave", null, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchesQuery("downtown"), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Name == "Downtown Branch");
        Assert.Contains(result.Items, b => b.Name == "Riverside");
    }

    [Fact]
    public async Task Handle_PaginatesResults()
    {
        for (var i = 1; i <= 5; i++)
        {
            _context.Branches.Add(Branch.Create($"Branch {i:00}", "Address", null, null));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchesQuery(null, PageNumber: 2, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal("Branch 03", result.Items[0].Name);
    }
}
