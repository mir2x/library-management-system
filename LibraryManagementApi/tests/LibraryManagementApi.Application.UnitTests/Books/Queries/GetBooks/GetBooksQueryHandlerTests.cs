using LibraryManagementApi.Application.Books.Queries.GetBooks;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Books.Queries.GetBooks;

public class GetBooksQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetBooksQueryHandler _handler;

    public GetBooksQueryHandlerTests()
    {
        _handler = new GetBooksQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ExcludesDeactivatedBooks()
    {
        var active = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var inactive = Book.Create("Old Book", "Some Author", "9780000000002", "Fiction", 1999, null);
        inactive.Deactivate();
        _context.Books.AddRange(active, inactive);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBooksQuery(null), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Clean Code", result.Items[0].Title);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByTitleAuthorIsbnOrGenre()
    {
        _context.Books.AddRange(
            Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null),
            Book.Create("The Clean Coder", "Robert C. Martin", "9780137081073", "Career", 2011, null),
            Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBooksQuery("clean"), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Title == "Clean Code");
        Assert.Contains(result.Items, b => b.Title == "The Clean Coder");
    }

    [Fact]
    public async Task Handle_PaginatesResults()
    {
        for (var i = 1; i <= 5; i++)
        {
            _context.Books.Add(Book.Create($"Book {i:00}", "Author", $"978000000000{i}", "Genre", 2000, null));
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetBooksQuery(null, PageNumber: 2, PageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal("Book 03", result.Items[0].Title);
    }
}
