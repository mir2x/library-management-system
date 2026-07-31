using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Books;
using LibraryManagementApi.Application.Books.Commands.CreateBook;

namespace LibraryManagementApi.Api.IntegrationTests.Books;

[Collection(IntegrationTestCollection.Name)]
public class BookEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateBook_AsAdmin_ReturnsCreated()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync(
            "/api/books", new CreateBookCommand("Clean Code", "Robert C. Martin", UniqueIsbn(), "Software Engineering", 2008, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_AsLibrarian_ReturnsForbidden()
    {
        var librarian = await CreateLibrarianClientAsync();

        var response = await librarian.PostAsJsonAsync(
            "/api/books", new CreateBookCommand("Clean Code", "Robert C. Martin", UniqueIsbn(), "Software Engineering", 2008, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithInvalidIsbn_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync(
            "/api/books", new CreateBookCommand("Clean Code", "Robert C. Martin", "not-an-isbn", "Software Engineering", 2008, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();
        var isbn = UniqueIsbn();
        await admin.PostAsJsonAsync("/api/books", new CreateBookCommand("Clean Code", "Robert C. Martin", isbn, "Software Engineering", 2008, null));

        var response = await admin.PostAsJsonAsync(
            "/api/books", new CreateBookCommand("Another Title", "Another Author", isbn, "Fiction", 2010, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetBookInventory_ThenGetBookById_ReflectsCopyCounts()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var book = await CreateBookAsync(admin);

        await SetBookInventoryAsync(admin, book.Id, branch.Id, 5);

        var response = await admin.GetAsync($"/api/books/{book.Id}");
        var detail = await response.Content.ReadFromJsonAsync<BookDetailDto>();

        Assert.Single(detail!.Inventory);
        Assert.Equal(5, detail.Inventory[0].TotalCopies);
        Assert.Equal(5, detail.Inventory[0].AvailableCopies);
        Assert.Equal(branch.Name, detail.Inventory[0].BranchName);
    }

    [Fact]
    public async Task UpdateBook_PatchOnlyProvidedFields_LeavesOthersUnchanged()
    {
        var admin = await CreateAdminClientAsync();
        var book = await CreateBookAsync(admin);

        var patchResponse = await admin.PatchAsJsonAsync(
            $"/api/books/{book.Id}",
            new { Title = (string?)null, Author = (string?)null, Genre = "Updated Genre", PublishedYear = (int?)null, Description = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var getResponse = await admin.GetAsync($"/api/books/{book.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<BookDetailDto>();
        Assert.Equal(book.Title, detail!.Title);
        Assert.Equal("Updated Genre", detail.Genre);
    }

    [Fact]
    public async Task DeleteBook_SoftDeletes_SetsIsActiveFalse()
    {
        var admin = await CreateAdminClientAsync();
        var book = await CreateBookAsync(admin);

        var deleteResponse = await admin.DeleteAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await admin.GetAsync($"/api/books/{book.Id}");
        var detail = await getResponse.Content.ReadFromJsonAsync<BookDetailDto>();
        Assert.False(detail!.IsActive);
    }

    [Fact]
    public async Task GetBooks_WithSearch_FiltersByTitle()
    {
        var admin = await CreateAdminClientAsync();
        var uniqueTitle = UniqueEmail("searchable-book");
        await CreateBookAsync(admin, uniqueTitle);
        await CreateBookAsync(admin);

        var response = await admin.GetAsync($"/api/books?Search={Uri.EscapeDataString(uniqueTitle)}&PageSize=50");
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<BookDto>>();

        Assert.Single(page!.Items);
        Assert.Equal(uniqueTitle, page.Items[0].Title);
    }
}
