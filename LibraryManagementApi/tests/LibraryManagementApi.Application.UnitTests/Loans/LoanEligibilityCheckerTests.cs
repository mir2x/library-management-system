using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans;

public class LoanEligibilityCheckerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly LoanEligibilityChecker _checker;

    public LoanEligibilityCheckerTests()
    {
        _checker = new LoanEligibilityChecker(_context);
    }

    [Fact]
    public async Task CheckAsync_WithActiveMemberBelowLimitAndNoDuplicate_ReturnsNull()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _checker.CheckAsync(member, Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CheckAsync_WithSuspendedMember_ReturnsReason()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        member.Suspend();
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _checker.CheckAsync(member, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("Member is not active and cannot borrow books.", result);
    }

    [Fact]
    public async Task CheckAsync_AtMaxActiveLoans_ReturnsReason()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        for (var i = 0; i < Loan.MaxActiveLoansPerMember; i++)
        {
            _context.Loans.Add(Loan.Create(member.Id, Guid.NewGuid(), Guid.NewGuid()));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _checker.CheckAsync(member, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal($"Member has reached the maximum of {Loan.MaxActiveLoansPerMember} active loans.", result);
    }

    [Fact]
    public async Task CheckAsync_WithExistingActiveLoanForSameBook_ReturnsReason()
    {
        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, Guid.NewGuid(), null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var bookId = Guid.NewGuid();
        _context.Loans.Add(Loan.Create(member.Id, bookId, Guid.NewGuid()));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _checker.CheckAsync(member, bookId, CancellationToken.None);

        Assert.Equal("Member already has an active loan for this book.", result);
    }
}
