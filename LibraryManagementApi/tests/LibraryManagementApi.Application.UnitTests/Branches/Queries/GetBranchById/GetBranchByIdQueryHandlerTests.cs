using LibraryManagementApi.Application.Branches.Queries.GetBranchById;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetBranchByIdQueryHandler _handler;

    public GetBranchByIdQueryHandlerTests()
    {
        _handler = new GetBranchByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingBranch_ReturnsDto()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", "555-0100", "downtown@library.org");
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchByIdQuery(branch.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(branch.Name, result!.Name);
    }

    [Fact]
    public async Task Handle_WithDeactivatedBranch_StillReturnsDto()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        branch.Deactivate();
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBranchByIdQuery(branch.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetBranchByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
