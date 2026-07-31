using LibraryManagementApi.Application.Members.Queries.GetMemberById;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMemberByIdQueryHandler _handler;

    public GetMemberByIdQueryHandlerTests()
    {
        _handler = new GetMemberByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingMember_ReturnsDtoWithBranchName()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMemberByIdQuery(member.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(member.FullName, result!.FullName);
        Assert.Equal(branch.Name, result.HomeBranchName);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetMemberByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
