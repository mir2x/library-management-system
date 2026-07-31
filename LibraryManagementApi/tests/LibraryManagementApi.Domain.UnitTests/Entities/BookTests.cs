using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Domain.UnitTests.Entities;

public class BookTests
{
    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToActive()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, "A handbook of agile software craftsmanship.");

        Assert.Equal("Clean Code", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal("9780132350884", book.Isbn);
        Assert.Equal("Software Engineering", book.Genre);
        Assert.Equal(2008, book.PublishedYear);
        Assert.True(book.IsActive);
    }

    [Fact]
    public void Update_ChangesMetadataButNotIsbn()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);

        book.Update("Clean Code (2nd Ed.)", "Robert C. Martin", "Programming", 2009, "Updated edition.");

        Assert.Equal("Clean Code (2nd Ed.)", book.Title);
        Assert.Equal("Programming", book.Genre);
        Assert.Equal(2009, book.PublishedYear);
        Assert.Equal("Updated edition.", book.Description);
        Assert.Equal("9780132350884", book.Isbn);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);

        book.Deactivate();

        Assert.False(book.IsActive);
    }
}
