using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Members.Commands.SuspendMember;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Application.UnitTests.Members.Commands.SuspendMember;

public class SuspendMemberCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly SuspendMemberCommandHandler _handler;

    public SuspendMemberCommandHandlerTests()
    {
        _handler = new SuspendMemberCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithActiveMember_SuspendsItAndReturnsSuccess()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new SuspendMemberCommand(member.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(MembershipStatus.Suspended, member.Status);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new SuspendMemberCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDeactivatedMember_ThrowsDomainException()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        member.Deactivate();
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new SuspendMemberCommand(member.Id);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
