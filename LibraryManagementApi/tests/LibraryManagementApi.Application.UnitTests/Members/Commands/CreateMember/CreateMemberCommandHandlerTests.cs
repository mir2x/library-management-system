using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Members.Commands.CreateMember;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly CreateMemberCommandHandler _handler;

    public CreateMemberCommandHandlerTests()
    {
        _handler = new CreateMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidBranchAndUniqueEmail_CreatesMemberWithNoLinkedAccount()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMemberCommand("Jane Doe", "jane.doe@example.com", "555-0100", "456 Elm St", branch.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(branch.Name, result.Value!.HomeBranchName);
        Assert.StartsWith("MEM-", result.Value!.MembershipNumber);

        var member = Assert.Single(_context.Members);
        Assert.Null(member.UserId);
    }

    [Fact]
    public async Task Handle_WithUnknownBranch_ThrowsNotFoundException()
    {
        var command = new CreateMemberCommand("Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDuplicateActiveEmail_ReturnsFailure()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        _context.Members.Add(Member.Create("MEM-00000001", "Existing Member", "jane.doe@example.com", null, null, branch.Id, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMemberCommand("Jane Doe", "JANE.DOE@example.com", null, null, branch.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["A member with this email already exists."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithSameEmailAsDeactivatedMember_Succeeds()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var deactivated = Member.Create("MEM-00000001", "Old Member", "jane.doe@example.com", null, null, branch.Id, null);
        deactivated.Deactivate();
        _context.Branches.Add(branch);
        _context.Members.Add(deactivated);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMemberCommand("Jane Doe", "jane.doe@example.com", null, null, branch.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }
}
