using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Members.Commands.ReactivateMember;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.ReactivateMember;

public class ReactivateMemberCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly ReactivateMemberCommandHandler _handler;

    public ReactivateMemberCommandHandlerTests()
    {
        _handler = new ReactivateMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithSuspendedMember_ReactivatesItAndReturnsSuccess()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        member.Suspend();
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new ReactivateMemberCommand(member.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(MembershipStatus.Active, member.Status);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new ReactivateMemberCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDeactivatedMember_ThrowsDomainException()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        member.Deactivate();
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReactivateMemberCommand(member.Id);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
