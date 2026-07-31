namespace LibraryManagementApi.Application.Books;

public static class IsbnHelper
{
    public static string Normalize(string isbn) => isbn.Replace("-", "").Replace(" ", "").ToUpperInvariant();

    // A pragmatic length/character-shape check (10 or 13 characters, digits with an optional
    // trailing 'X' for ISBN-10), not a full checksum validation — sufficient to catch obviously
    // malformed input without pulling in a dedicated ISBN library.
    public static bool IsValid(string isbn)
    {
        var normalized = Normalize(isbn);

        return normalized.Length is 10 or 13
            && normalized[..^1].All(char.IsDigit)
            && (char.IsDigit(normalized[^1]) || normalized[^1] == 'X');
    }
}
