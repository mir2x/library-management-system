using LibraryManagementApi.Application.Reports.Queries.GetMemberActivityReport;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetMemberActivityReport;

public class GetMemberActivityReportQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMemberActivityReportQueryHandler _handler;

    public GetMemberActivityReportQueryHandlerTests()
    {
        _handler = new GetMemberActivityReportQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ComputesActiveTotalOverdueAndReservationCounts()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var book2 = Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null);
        var book3 = Book.Create("The Pragmatic Programmer", "Andrew Hunt", "9780135957059", "Software Engineering", 2019, null);
        _context.Branches.Add(branch);
        _context.Books.AddRange(book1, book2, book3);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var activeLoan = Loan.Create(member.Id, book1.Id, branch.Id);
        var overdueLoan = Loan.Create(member.Id, book2.Id, branch.Id);
        var returnedLoan = Loan.Create(member.Id, book3.Id, branch.Id);
        _context.Loans.AddRange(activeLoan, overdueLoan, returnedLoan);
        await _context.SaveChangesAsync(CancellationToken.None);

        returnedLoan.MarkReturned();
        _context.Entry(overdueLoan).Property(nameof(Loan.DueDateUtc)).CurrentValue = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Reservations.Add(Reservation.Create(member.Id, book3.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMemberActivityReportQuery(null), CancellationToken.None);

        Assert.Single(result.Items);
        var dto = result.Items[0];
        Assert.Equal(member.FullName, dto.MemberName);
        Assert.Equal(member.MembershipNumber, dto.MembershipNumber);
        Assert.Equal(2, dto.ActiveLoanCount);
        Assert.Equal(3, dto.TotalLoanCount);
        Assert.Equal(1, dto.OverdueLoanCount);
        Assert.Equal(1, dto.ActiveReservationCount);
    }

    [Fact]
    public async Task Handle_WithBranchFilter_ExcludesOtherBranchMembers()
    {
        var branch1 = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var branch2 = Branch.Create("Uptown Branch", "456 Side St", null, null);
        _context.Branches.AddRange(branch1, branch2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member1 = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch1.Id, null);
        var member2 = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch2.Id, null);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMemberActivityReportQuery(branch1.Id), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(member1.FullName, result.Items[0].MemberName);
    }
}
