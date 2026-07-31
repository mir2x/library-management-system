using LibraryManagementApi.Application.Branches.Commands.CreateBranch;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CreateBranchCommandHandler _handler;

    public CreateBranchCommandHandlerTests()
    {
        _handler = new CreateBranchCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithUniqueName_CreatesBranchAndReturnsDto()
    {
        var command = new CreateBranchCommand("Downtown Branch", "123 Main St", "555-0100", "downtown@library.org");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(command.Name, result.Value!.Name);
        Assert.Equal(command.Address, result.Value!.Address);
        Assert.True(result.Value!.IsActive);
        Assert.Single(_context.Branches);
    }

    [Fact]
    public async Task Handle_WithDuplicateActiveName_ReturnsFailure()
    {
        _context.Branches.Add(Branch.Create("Downtown Branch", "Some address", null, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateBranchCommand("downtown branch", "123 Main St", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["A branch with this name already exists."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithSameNameAsDeactivatedBranch_Succeeds()
    {
        var deactivated = Branch.Create("Downtown Branch", "Some address", null, null);
        deactivated.Deactivate();
        _context.Branches.Add(deactivated);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateBranchCommand("Downtown Branch", "123 Main St", null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
