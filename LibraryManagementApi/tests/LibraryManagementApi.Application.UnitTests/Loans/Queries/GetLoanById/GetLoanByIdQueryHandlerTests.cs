using LibraryManagementApi.Application.Loans.Queries.GetLoanById;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans.Queries.GetLoanById;

public class GetLoanByIdQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetLoanByIdQueryHandler _handler;

    public GetLoanByIdQueryHandlerTests()
    {
        _handler = new GetLoanByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingLoan_ReturnsDtoWithJoinedNames()
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

        var result = await _handler.Handle(new GetLoanByIdQuery(loan.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(member.FullName, result!.MemberName);
        Assert.Equal(book.Title, result.BookTitle);
        Assert.Equal(branch.Name, result.BranchName);
        Assert.False(result.IsOverdue);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetLoanByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
