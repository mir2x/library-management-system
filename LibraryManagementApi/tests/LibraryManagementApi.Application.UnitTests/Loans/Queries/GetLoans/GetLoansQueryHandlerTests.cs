using LibraryManagementApi.Application.Loans.Queries.GetLoans;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans.Queries.GetLoans;

public class GetLoansQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetLoansQueryHandler _handler;

    private async Task<(Member Member1, Member Member2, Book Book1, Book Book2, Branch Branch)> SeedAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var book2 = Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null);
        _context.Branches.Add(branch);
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member1 = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        var member2 = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member1, member2, book1, book2, branch);
    }

    public GetLoansQueryHandlerTests()
    {
        _handler = new GetLoansQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithMemberIdFilter_ReturnsOnlyThatMembersLoans()
    {
        var (member1, member2, book1, book2, branch) = await SeedAsync();
        _context.Loans.AddRange(
            Loan.Create(member1.Id, book1.Id, branch.Id),
            Loan.Create(member2.Id, book2.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetLoansQuery(member1.Id, null, null, null), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(member1.FullName, result.Items[0].MemberName);
    }

    [Fact]
    public async Task Handle_WithBookIdFilter_ReturnsOnlyLoansForThatBook()
    {
        var (member1, member2, book1, book2, branch) = await SeedAsync();
        _context.Loans.AddRange(
            Loan.Create(member1.Id, book1.Id, branch.Id),
            Loan.Create(member2.Id, book2.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetLoansQuery(null, book2.Id, null, null), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(book2.Title, result.Items[0].BookTitle);
    }

    [Fact]
    public async Task Handle_WithOnlyOverdueFilter_ExcludesLoansNotYetOverdue()
    {
        var (member1, _, book1, _, branch) = await SeedAsync();
        var loan = Loan.Create(member1.Id, book1.Id, branch.Id);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Freshly created loans are due 14 days out, so they should never appear when
        // OnlyOverdue is requested.
        var result = await _handler.Handle(new GetLoansQuery(null, null, null, OnlyOverdue: true), CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_WithOnlyOverdueFilter_IncludesActualOverdueLoan()
    {
        var (member1, _, book1, _, branch) = await SeedAsync();
        var loan = Loan.Create(member1.Id, book1.Id, branch.Id);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Backdate via EF's change tracker (bypassing the entity's private setter, which has no
        // public way to simulate time passing) to exercise the overdue filter for real.
        _context.Entry(loan).Property(nameof(Loan.DueDateUtc)).CurrentValue = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetLoansQuery(null, null, null, OnlyOverdue: true), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.True(result.Items[0].IsOverdue);
    }
}
