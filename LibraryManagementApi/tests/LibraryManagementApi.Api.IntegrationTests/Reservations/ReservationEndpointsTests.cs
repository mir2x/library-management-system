using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Loans;
using LibraryManagementApi.Application.Loans.Commands.BorrowBook;
using LibraryManagementApi.Application.Reservations;
using LibraryManagementApi.Application.Reservations.Commands.CreateReservation;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Api.IntegrationTests.Reservations;

[Collection(IntegrationTestCollection.Name)]
public class ReservationEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(Application.Branches.BranchDto Branch, Application.Books.BookDto Book, Application.Members.MemberDto Borrower)>
        SeedFullyCheckedOutBookAsync(HttpClient admin, HttpClient librarian)
    {
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, borrower) = await RegisterMemberWithProfileAsync(branch.Id);
        var borrowResponse = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(borrower.Id, book.Id, branch.Id));
        borrowResponse.EnsureSuccessStatusCode();

        return (branch, book, borrower);
    }

    [Fact]
    public async Task CreateReservation_ForFullyCheckedOutBook_Succeeds()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, _) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (_, waitingMember) = await RegisterMemberWithProfileAsync(branch.Id);

        var response = await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(waitingMember.Id, book.Id, branch.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reservation = await response.Content.ReadFromJsonWithEnumsAsync<ReservationDto>();
        Assert.Equal(ReservationStatus.Pending, reservation!.Status);
    }

    [Fact]
    public async Task ReturnBook_WithPendingReservation_MarksItReadyWithoutFreeingInventory()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, borrower) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (_, waitingMember) = await RegisterMemberWithProfileAsync(branch.Id);
        var reservationResponse = await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(waitingMember.Id, book.Id, branch.Id));
        var reservation = await reservationResponse.Content.ReadFromJsonWithEnumsAsync<ReservationDto>();

        var loansPage = await librarian.GetFromJsonWithEnumsAsync<PagedResponse<LoanDto>>($"/api/loans?MemberId={borrower.Id}");
        var loan = loansPage!.Items.Single();
        var returnResponse = await librarian.PostAsync($"/api/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.NoContent, returnResponse.StatusCode);

        var afterReturn = await librarian.GetFromJsonWithEnumsAsync<ReservationDto>($"/api/reservations/{reservation!.Id}");
        Assert.Equal(ReservationStatus.Ready, afterReturn!.Status);

        var bookDetail = await librarian.GetFromJsonAsync<Application.Books.BookDetailDto>($"/api/books/{book.Id}");
        Assert.Equal(0, bookDetail!.Inventory[0].AvailableCopies);
    }

    [Fact]
    public async Task FulfillReservation_WhenReady_CreatesLoanForWaitingMember()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, borrower) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (_, waitingMember) = await RegisterMemberWithProfileAsync(branch.Id);
        var reservationResponse = await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(waitingMember.Id, book.Id, branch.Id));
        var reservation = await reservationResponse.Content.ReadFromJsonWithEnumsAsync<ReservationDto>();

        var loansPage = await librarian.GetFromJsonWithEnumsAsync<PagedResponse<LoanDto>>($"/api/loans?MemberId={borrower.Id}");
        await librarian.PostAsync($"/api/loans/{loansPage!.Items.Single().Id}/return", null);

        var fulfillResponse = await librarian.PostAsync($"/api/reservations/{reservation!.Id}/fulfill", null);

        Assert.Equal(HttpStatusCode.OK, fulfillResponse.StatusCode);
        var newLoan = await fulfillResponse.Content.ReadFromJsonWithEnumsAsync<LoanDto>();
        Assert.Equal(waitingMember.Id, newLoan!.MemberId);
        Assert.Equal(book.Id, newLoan.BookId);
    }

    [Fact]
    public async Task CreateMyReservation_SelfService_Succeeds()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, _) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (auth, _) = await RegisterMemberWithProfileAsync(branch.Id);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.PostAsJsonAsync("/api/reservations/me", new { BookId = book.Id, BranchId = branch.Id });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_ByOwningMember_Succeeds()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, _) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (auth, waitingMember) = await RegisterMemberWithProfileAsync(branch.Id);
        var reservationResponse = await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(waitingMember.Id, book.Id, branch.Id));
        var reservation = await reservationResponse.Content.ReadFromJsonWithEnumsAsync<ReservationDto>();

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.PostAsync($"/api/reservations/{reservation!.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CancelReservation_ByAnotherMember_ReturnsForbidden()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book, _) = await SeedFullyCheckedOutBookAsync(admin, librarian);
        var (_, waitingMember) = await RegisterMemberWithProfileAsync(branch.Id);
        var reservationResponse = await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(waitingMember.Id, book.Id, branch.Id));
        var reservation = await reservationResponse.Content.ReadFromJsonWithEnumsAsync<ReservationDto>();

        var (otherAuth, _) = await RegisterMemberWithProfileAsync(branch.Id);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", otherAuth.AccessToken);
        var response = await client.PostAsync($"/api/reservations/{reservation!.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
