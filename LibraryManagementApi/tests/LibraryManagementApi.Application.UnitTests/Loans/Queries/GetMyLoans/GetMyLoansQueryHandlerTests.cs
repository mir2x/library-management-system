using LibraryManagementApi.Application.Loans.Queries.GetMyLoans;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans.Queries.GetMyLoans;

public class GetMyLoansQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMyLoansQueryHandler _handler;

    public GetMyLoansQueryHandlerTests()
    {
        _handler = new GetMyLoansQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithLinkedMemberAndLoans_ReturnsOnlyThatMembersLoans()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var me = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, userId: "user-1");
        var someoneElse = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, userId: "user-2");
        _context.Members.AddRange(me, someoneElse);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Loans.AddRange(
            Loan.Create(me.Id, book.Id, branch.Id),
            Loan.Create(someoneElse.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMyLoansQuery("user-1"), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(me.FullName, result.Items[0].MemberName);
    }

    [Fact]
    public async Task Handle_WithNoLinkedMember_ReturnsEmptyPage()
    {
        var result = await _handler.Handle(new GetMyLoansQuery("user-without-member"), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
