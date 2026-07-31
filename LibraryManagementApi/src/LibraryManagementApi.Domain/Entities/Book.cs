using LibraryManagementApi.Domain.Common;

namespace LibraryManagementApi.Domain.Entities;

public class Book : BaseAuditableEntity
{
    private Book()
    {
    }

    private Book(string title, string author, string isbn, string genre, int publishedYear, string? description)
    {
        Title = title;
        Author = author;
        Isbn = isbn;
        Genre = genre;
        PublishedYear = publishedYear;
        Description = description;
        IsActive = true;
    }

    public string Title { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    // Immutable after creation: the ISBN identifies a specific edition, so changing it post
    // creation would really mean "this is a different book", not an update.
    public string Isbn { get; private set; } = string.Empty;

    public string Genre { get; private set; } = string.Empty;

    public int PublishedYear { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Book Create(string title, string author, string isbn, string genre, int publishedYear, string? description) =>
        new(title, author, isbn, genre, publishedYear, description);

    public void Update(string title, string author, string genre, int publishedYear, string? description)
    {
        Title = title;
        Author = author;
        Genre = genre;
        PublishedYear = publishedYear;
        Description = description;
    }

    public void Deactivate() => IsActive = false;
}
