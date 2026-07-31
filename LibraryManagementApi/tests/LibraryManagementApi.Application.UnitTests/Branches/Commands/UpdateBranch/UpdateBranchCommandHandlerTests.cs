using LibraryManagementApi.Application.Branches.Commands.UpdateBranch;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly UpdateBranchCommandHandler _handler;

    public UpdateBranchCommandHandlerTests()
    {
        _handler = new UpdateBranchCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithPartialFields_UpdatesOnlyProvidedFieldsAndLeavesOthersUnchanged()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", "555-0100", "downtown@library.org");
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateBranchCommand(branch.Id, "Uptown Branch", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Uptown Branch", branch.Name);
        Assert.Equal("123 Main St", branch.Address);
        Assert.Equal("555-0100", branch.ContactNumber);
        Assert.Equal("downtown@library.org", branch.Email);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new UpdateBranchCommand(Guid.NewGuid(), "New Name", null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNameMatchingAnotherActiveBranch_ReturnsFailure()
    {
        var existing = Branch.Create("Uptown Branch", "456 Elm St", null, null);
        var target = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.AddRange(existing, target);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateBranchCommand(target.Id, "uptown branch", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["A branch with this name already exists."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithSameNameUnchanged_DoesNotTriggerUniquenessFailure()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateBranchCommand(branch.Id, "Downtown Branch", "456 New Address", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("456 New Address", branch.Address);
    }
}
