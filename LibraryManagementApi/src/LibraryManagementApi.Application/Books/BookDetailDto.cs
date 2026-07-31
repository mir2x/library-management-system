namespace LibraryManagementApi.Application.Books;

public record BookDetailDto(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    string Genre,
    int PublishedYear,
    string? Description,
    bool IsActive,
    IReadOnlyList<BookInventoryDto> Inventory);
