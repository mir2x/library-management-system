using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Loans.Commands.BorrowBook;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Loans.Commands.BorrowBook;

public class BorrowBookCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _handler = new BorrowBookCommandHandler(_context);
    }

    private async Task<(Member Member, Book Book, Branch Branch)> SeedMemberBookBranchWithInventoryAsync(int availableCopies = 1)
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        _context.Members.Add(member);
        await _context.SaveChangesAsync(CancellationToken.None);

        if (availableCopies > 0)
        {
            _context.BookInventories.Add(BookInventory.Create(book.Id, branch.Id, availableCopies));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return (member, book, branch);
    }

    [Fact]
    public async Task Handle_WithEverythingValid_CreatesLoanAndDecrementsInventory()
    {
        var (member, book, branch) = await SeedMemberBookBranchWithInventoryAsync();

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, book.Id, branch.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(member.FullName, result.Value!.MemberName);
        Assert.Equal(book.Title, result.Value!.BookTitle);

        var inventory = Assert.Single(_context.BookInventories);
        Assert.Equal(0, inventory.AvailableCopies);
    }

    [Fact]
    public async Task Handle_WithUnknownMember_ThrowsNotFoundException()
    {
        var (_, book, branch) = await SeedMemberBookBranchWithInventoryAsync();

        var command = new BorrowBookCommand(Guid.NewGuid(), book.Id, branch.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSuspendedMember_ReturnsFailure()
    {
        var (member, book, branch) = await SeedMemberBookBranchWithInventoryAsync();
        member.Suspend();
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, book.Id, branch.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Member is not active and cannot borrow books."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithUnknownBook_ThrowsNotFoundException()
    {
        var (member, _, branch) = await SeedMemberBookBranchWithInventoryAsync();

        var command = new BorrowBookCommand(member.Id, Guid.NewGuid(), branch.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithUnknownBranch_ThrowsNotFoundException()
    {
        var (member, book, _) = await SeedMemberBookBranchWithInventoryAsync();

        var command = new BorrowBookCommand(member.Id, book.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNoInventoryRecord_ReturnsFailure()
    {
        var (member, book, branch) = await SeedMemberBookBranchWithInventoryAsync(availableCopies: 0);

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, book.Id, branch.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["No copies of this book are available at this branch."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithNoAvailableCopies_ReturnsFailure()
    {
        var (member, book, branch) = await SeedMemberBookBranchWithInventoryAsync(availableCopies: 1);
        var inventory = Assert.Single(_context.BookInventories);
        inventory.Borrow(); // consume the only copy
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, book.Id, branch.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["No copies of this book are available at this branch."], result.Errors);
    }

    [Fact]
    public async Task Handle_WhenMemberAtMaxActiveLoans_ReturnsFailure()
    {
        var (member, _, branch) = await SeedMemberBookBranchWithInventoryAsync(availableCopies: 100);

        for (var i = 0; i < Loan.MaxActiveLoansPerMember; i++)
        {
            var otherBook = Book.Create($"Book {i}", "Author", $"978000000000{i}", "Genre", 2000, null);
            _context.Books.Add(otherBook);
            await _context.SaveChangesAsync(CancellationToken.None);
            _context.BookInventories.Add(BookInventory.Create(otherBook.Id, branch.Id, 5));
            _context.Loans.Add(Loan.Create(member.Id, otherBook.Id, branch.Id));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var newBook = Book.Create("One More Book", "Author", "9780000000099", "Genre", 2000, null);
        _context.Books.Add(newBook);
        await _context.SaveChangesAsync(CancellationToken.None);
        _context.BookInventories.Add(BookInventory.Create(newBook.Id, branch.Id, 5));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, newBook.Id, branch.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal([$"Member has reached the maximum of {Loan.MaxActiveLoansPerMember} active loans."], result.Errors);
    }

    [Fact]
    public async Task Handle_WithExistingActiveLoanForSameBook_ReturnsFailure()
    {
        var (member, book, branch) = await SeedMemberBookBranchWithInventoryAsync(availableCopies: 5);
        _context.Loans.Add(Loan.Create(member.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new BorrowBookCommand(member.Id, book.Id, branch.Id), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(["Member already has an active loan for this book."], result.Errors);
    }
}
