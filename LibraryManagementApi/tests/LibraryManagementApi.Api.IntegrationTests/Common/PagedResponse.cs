namespace LibraryManagementApi.Api.IntegrationTests.Common;

// Mirrors Application.Common.Models.PaginatedList<T>'s serialized shape (its properties, not its
// constructor). System.Text.Json's parameterized-constructor deserializer requires every ctor
// parameter to bind to a property, and PaginatedList<T>'s ctor takes a pageSize it never exposes
// as a property — so deserializing straight into it on the test/client side fails. This type has
// no such mismatch.
public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int TotalPages,
    int TotalCount,
    bool HasPreviousPage,
    bool HasNextPage);
