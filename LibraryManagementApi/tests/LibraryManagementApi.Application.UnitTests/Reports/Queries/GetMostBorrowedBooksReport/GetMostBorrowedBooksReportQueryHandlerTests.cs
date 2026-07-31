using LibraryManagementApi.Application.Reports.Queries.GetMostBorrowedBooksReport;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetMostBorrowedBooksReport;

public class GetMostBorrowedBooksReportQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMostBorrowedBooksReportQueryHandler _handler;

    public GetMostBorrowedBooksReportQueryHandlerTests()
    {
        _handler = new GetMostBorrowedBooksReportQueryHandler(_context);
    }

    private async Task<(Member Member, Book Popular, Book Unpopular, Branch Branch)> SeedAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var popular = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var unpopular = Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null);
        _context.Branches.Add(branch);
        _context.Books.AddRange(popular, unpopular);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member, popular, unpopular, branch);
    }

    [Fact]
    public async Task Handle_OrdersByBorrowCountDescending()
    {
        var (member, popular, unpopular, branch) = await SeedAsync();
        _context.Loans.AddRange(
            Loan.Create(member.Id, popular.Id, branch.Id),
            Loan.Create(member.Id, popular.Id, branch.Id),
            Loan.Create(member.Id, unpopular.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMostBorrowedBooksReportQuery(null, null, null), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(popular.Id, result[0].BookId);
        Assert.Equal(2, result[0].BorrowCount);
        Assert.Equal(unpopular.Id, result[1].BookId);
        Assert.Equal(1, result[1].BorrowCount);
    }

    [Fact]
    public async Task Handle_WithTopLimit_TruncatesResults()
    {
        var (member, popular, unpopular, branch) = await SeedAsync();
        _context.Loans.AddRange(
            Loan.Create(member.Id, popular.Id, branch.Id),
            Loan.Create(member.Id, unpopular.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMostBorrowedBooksReportQuery(null, null, null, Top: 1), CancellationToken.None);

        Assert.Single(result);
    }

    [Fact]
    public async Task Handle_WithBranchFilter_OnlyCountsThatBranchsLoans()
    {
        var (member, popular, _, branch) = await SeedAsync();
        var otherBranch = Branch.Create("Uptown Branch", "456 Side St", null, null);
        _context.Branches.Add(otherBranch);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Loans.AddRange(
            Loan.Create(member.Id, popular.Id, branch.Id),
            Loan.Create(member.Id, popular.Id, otherBranch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMostBorrowedBooksReportQuery(branch.Id, null, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(1, result[0].BorrowCount);
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ExcludesLoansOutsideRange()
    {
        var (member, popular, _, branch) = await SeedAsync();
        var loan = Loan.Create(member.Id, popular.Id, branch.Id);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Entry(loan).Property(nameof(Loan.BorrowedAtUtc)).CurrentValue = DateTime.UtcNow.AddDays(-30);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(
            new GetMostBorrowedBooksReportQuery(null, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow), CancellationToken.None);

        Assert.Empty(result);
    }
}
