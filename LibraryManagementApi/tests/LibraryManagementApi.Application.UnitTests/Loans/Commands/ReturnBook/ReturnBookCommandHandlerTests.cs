using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Loans.Commands.ReturnBook;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;
using LibraryManagementApi.Domain.Exceptions;

namespace LibraryManagementApi.Application.UnitTests.Loans.Commands.ReturnBook;

public class ReturnBookCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly ReturnBookCommandHandler _handler;

    public ReturnBookCommandHandlerTests()
    {
        _handler = new ReturnBookCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithActiveLoan_MarksReturnedAndIncrementsAvailableCopies()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var inventory = BookInventory.Create(book.Id, branch.Id, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);

        var loan = Loan.Create(member.Id, book.Id, branch.Id);
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new ReturnBookCommand(loan.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.NotNull(loan.ReturnedAtUtc);
        Assert.Equal(1, inventory.AvailableCopies);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ThrowsNotFoundException()
    {
        var command = new ReturnBookCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyReturnedLoan_ThrowsDomainException()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        var inventory = BookInventory.Create(book.Id, branch.Id, 1);
        inventory.Borrow();
        _context.BookInventories.Add(inventory);

        var loan = Loan.Create(member.Id, book.Id, branch.Id);
        loan.MarkReturned();
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReturnBookCommand(loan.Id);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
