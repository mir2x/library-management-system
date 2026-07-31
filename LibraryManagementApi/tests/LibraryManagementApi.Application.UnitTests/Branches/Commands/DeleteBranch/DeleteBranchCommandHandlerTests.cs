using LibraryManagementApi.Application.Branches.Commands.DeleteBranch;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly DeleteBranchCommandHandler _handler;

    public DeleteBranchCommandHandlerTests()
    {
        _handler = new DeleteBranchCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingBranch_DeactivatesItAndReturnsSuccess()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new DeleteBranchCommand(branch.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(branch.IsActive);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new DeleteBranchCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
