using LibraryManagementApi.Application.Members.Queries.GetMembers;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _handler = new GetMembersQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ExcludesDeactivatedMembersButIncludesSuspended()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var active = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        var suspended = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        suspended.Suspend();
        var deactivated = Member.Create("MEM-00000003", "Old Member", "old@example.com", null, null, branch.Id, null);
        deactivated.Deactivate();

        _context.Members.AddRange(active, suspended, deactivated);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMembersQuery(null), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, m => m.FullName == "Jane Doe");
        Assert.Contains(result.Items, m => m.FullName == "John Smith");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByNameEmailOrMembershipNumber()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        _context.Members.AddRange(
            Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null),
            Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMembersQuery("jane"), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Jane Doe", result.Items[0].FullName);
    }

    [Fact]
    public async Task Handle_PaginatesResults()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        for (var i = 1; i <= 5; i++)
        {
            _context.Members.Add(Member.Create($"MEM-{i:00000000}", $"Member {i:00}", $"member{i}@example.com", null, null, branch.Id, null));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMembersQuery(null, PageNumber: 2, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal("Member 03", result.Items[0].FullName);
    }
}
