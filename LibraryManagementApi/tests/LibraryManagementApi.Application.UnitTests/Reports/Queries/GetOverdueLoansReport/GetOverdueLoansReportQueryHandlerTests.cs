using LibraryManagementApi.Application.Reports.Queries.GetOverdueLoansReport;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetOverdueLoansReport;

public class GetOverdueLoansReportQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetOverdueLoansReportQueryHandler _handler;

    public GetOverdueLoansReportQueryHandlerTests()
    {
        _handler = new GetOverdueLoansReportQueryHandler(_context);
    }

    private async Task<(Member Member, Book Book, Branch Branch, Loan Loan)> SeedOverdueLoanAsync(int daysOverdue = 3)
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var loan = Loan.Create(member.Id, book.Id, branch.Id);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Entry(loan).Property(nameof(Loan.DueDateUtc)).CurrentValue = DateTime.UtcNow.AddDays(-daysOverdue);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member, book, branch, loan);
    }

    [Fact]
    public async Task Handle_WithNoOverdueLoans_ReturnsEmpty()
    {
        var result = await _handler.Handle(new GetOverdueLoansReportQuery(null), CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_WithOverdueLoan_ReturnsItWithJoinedNamesAndDaysOverdue()
    {
        var (member, book, branch, _) = await SeedOverdueLoanAsync(daysOverdue: 3);

        var result = await _handler.Handle(new GetOverdueLoansReportQuery(null), CancellationToken.None);

        Assert.Single(result.Items);
        var dto = result.Items[0];
        Assert.Equal(member.FullName, dto.MemberName);
        Assert.Equal(book.Title, dto.BookTitle);
        Assert.Equal(branch.Name, dto.BranchName);
        Assert.Equal(3, dto.DaysOverdue);
    }

    [Fact]
    public async Task Handle_WithBranchFilter_ExcludesOtherBranches()
    {
        await SeedOverdueLoanAsync();

        var result = await _handler.Handle(new GetOverdueLoansReportQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_WithLoanNotYetDue_IsExcluded()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Loans.Add(Loan.Create(member.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetOverdueLoansReportQuery(null), CancellationToken.None);

        Assert.Empty(result.Items);
    }
}
