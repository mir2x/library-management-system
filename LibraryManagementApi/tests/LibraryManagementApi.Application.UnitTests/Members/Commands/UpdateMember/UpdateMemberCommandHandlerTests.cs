using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Members.Commands.UpdateMember;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _handler = new UpdateMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithPartialFields_UpdatesOnlyProvidedFieldsAndLeavesOthersUnchanged()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", "555-0100", "456 Elm St", branch.Id, null);
        _context.Branches.Add(branch);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand(member.Id, "Jane R. Doe", null, null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Jane R. Doe", member.FullName);
        Assert.Equal("jane.doe@example.com", member.Email);
        Assert.Equal("555-0100", member.Phone);
        Assert.Equal("456 Elm St", member.Address);
        Assert.Equal(branch.Id, member.HomeBranchId);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new UpdateMemberCommand(Guid.NewGuid(), "New Name", null, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownHomeBranchId_ThrowsNotFoundException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Branches.Add(branch);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand(member.Id, null, null, null, null, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmailMatchingAnotherActiveMember_ReturnsFailure()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var existing = Member.Create("MEM-00000001", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        var target = Member.Create("MEM-00000002", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Branches.Add(branch);
        _context.Members.AddRange(existing, target);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateMemberCommand(target.Id, null, "john.smith@example.com", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["A member with this email already exists."], result.Errors);
    }
}
