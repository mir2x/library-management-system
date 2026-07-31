namespace LibraryManagementApi.Application.Reports;

public record MostBorrowedBookDto(
    Guid BookId,
    string Title,
    string Author,
    int BorrowCount);
