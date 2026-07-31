using LibraryManagementApi.Application.Members.Queries.GetMyMemberProfile;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Members.Queries.GetMyMemberProfile;

public class GetMyMemberProfileQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMyMemberProfileQueryHandler _handler;

    public GetMyMemberProfileQueryHandlerTests()
    {
        _handler = new GetMyMemberProfileQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithLinkedMember_ReturnsDto()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, userId: "user-1");
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMyMemberProfileQuery("user-1"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(member.MembershipNumber, result!.MembershipNumber);
    }

    [Fact]
    public async Task Handle_WithNoLinkedMember_ReturnsNull()
    {
        var result = await _handler.Handle(new GetMyMemberProfileQuery("user-without-member"), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithWalkInMembersHavingNoUserId_DoesNotMatchThem()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var walkIn = Member.Create("MEM-00000001", "Walk In Member", "walkin@example.com", null, null, branch.Id, userId: null);
        _context.Members.Add(walkIn);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMyMemberProfileQuery("some-user-id"), CancellationToken.None);

        Assert.Null(result);
    }
}
