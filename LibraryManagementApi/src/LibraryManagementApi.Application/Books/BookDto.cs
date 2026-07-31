namespace LibraryManagementApi.Application.Books;

public record BookDto(Guid Id, string Title, string Author, string Isbn, string Genre, int PublishedYear, string? Description, bool IsActive);
