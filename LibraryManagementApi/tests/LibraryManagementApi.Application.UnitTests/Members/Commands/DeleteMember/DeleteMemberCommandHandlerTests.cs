using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Members.Commands.DeleteMember;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly DeleteMemberCommandHandler _handler;

    public DeleteMemberCommandHandlerTests()
    {
        _handler = new DeleteMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingMember_DeactivatesItAndReturnsSuccess()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new DeleteMemberCommand(member.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(MembershipStatus.Deactivated, member.Status);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new DeleteMemberCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
