using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.Loans.Commands.BorrowBook;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Api.IntegrationTests.Loans;

[Collection(IntegrationTestCollection.Name)]
public class LoanEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task BorrowBook_WithAvailableCopy_CreatesLoanAndDecrementsInventory()
    {
        var admin = await CreateAdminClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var librarian = await CreateLibrarianClientAsync();

        var response = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loan = await response.Content.ReadFromJsonWithEnumsAsync<LoanDto>();
        Assert.Equal(LoanStatus.Active, loan!.Status);

        var bookDetail = await librarian.GetFromJsonAsync<Application.Books.BookDetailDto>($"/api/books/{book.Id}");
        Assert.Equal(0, bookDetail!.Inventory[0].AvailableCopies);
    }

    [Fact]
    public async Task BorrowBook_WithNoAvailableCopies_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 0);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var librarian = await CreateLibrarianClientAsync();

        var response = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_SameBookTwiceForSameMember_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 2);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var librarian = await CreateLibrarianClientAsync();

        var first = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_ExceedingMaxActiveLoans_ReturnsBadRequest()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var branch = await CreateBranchAsync(admin);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);

        for (var i = 0; i < 5; i++)
        {
            var book = await CreateBookAsync(admin);
            await SetBookInventoryAsync(admin, book.Id, branch.Id, 1);
            var response = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var extraBook = await CreateBookAsync(admin);
        await SetBookInventoryAsync(admin, extraBook.Id, branch.Id, 1);
        var sixthResponse = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, extraBook.Id, branch.Id));

        Assert.Equal(HttpStatusCode.BadRequest, sixthResponse.StatusCode);
    }

    [Fact]
    public async Task ReturnBook_ReleasesInventoryAndMarksReturned()
    {
        var admin = await CreateAdminClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var librarian = await CreateLibrarianClientAsync();

        var borrowResponse = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));
        var loan = await borrowResponse.Content.ReadFromJsonWithEnumsAsync<LoanDto>();

        var returnResponse = await librarian.PostAsync($"/api/loans/{loan!.Id}/return", null);
        Assert.Equal(HttpStatusCode.NoContent, returnResponse.StatusCode);

        var afterReturn = await librarian.GetFromJsonWithEnumsAsync<LoanDto>($"/api/loans/{loan.Id}");
        Assert.Equal(LoanStatus.Returned, afterReturn!.Status);

        var bookDetail = await librarian.GetFromJsonAsync<Application.Books.BookDetailDto>($"/api/books/{book.Id}");
        Assert.Equal(1, bookDetail!.Inventory[0].AvailableCopies);
    }

    [Fact]
    public async Task GetMyLoans_ReturnsOwnLoanHistory()
    {
        var admin = await CreateAdminClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (auth, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var librarian = await CreateLibrarianClientAsync();
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.GetAsync("/api/loans/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonWithEnumsAsync<PagedResponse<LoanDto>>();
        Assert.Single(page!.Items);
        Assert.Equal(book.Title, page.Items[0].BookTitle);
    }
}
