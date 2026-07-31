using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Loans.Commands.BorrowBook;
using LibraryManagementApi.Application.Reports;
using LibraryManagementApi.Application.Reservations.Commands.CreateReservation;
using LibraryManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementApi.Api.IntegrationTests.Reports;

// These exercise real Postgres, not the InMemory provider the Application unit tests use —
// the reports handlers rely on correlated Count() subqueries, GroupBy with conditional
// aggregates, and DateTime.Date arithmetic, none of which are guaranteed to translate the
// same way across providers.
[Collection(IntegrationTestCollection.Name)]
public class ReportEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetOverdueLoansReport_ReturnsBackdatedLoan_ScopedToBranch()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        var borrowResponse = await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));
        var loan = await borrowResponse.Content.ReadFromJsonAsync<Application.Loans.LoanDto>();

        await ExecuteDbContextAsync(async context =>
        {
            var entity = await context.Loans.SingleAsync(l => l.Id == loan!.Id);
            context.Entry(entity).Property(nameof(Loan.DueDateUtc)).CurrentValue = DateTime.UtcNow.AddDays(-2);
            await context.SaveChangesAsync();
        });

        var response = await admin.GetAsync($"/api/reports/overdue-loans?BranchId={branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<OverdueLoanDto>>();
        var item = Assert.Single(page!.Items);
        Assert.Equal(loan!.Id, item.LoanId);
        Assert.Equal(2, item.DaysOverdue);
    }

    [Fact]
    public async Task GetMostBorrowedBooksReport_OrdersByBorrowCount_ScopedToBranch()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var branch = await CreateBranchAsync(admin);
        var popular = await CreateBookAsync(admin);
        var unpopular = await CreateBookAsync(admin);
        await SetBookInventoryAsync(admin, popular.Id, branch.Id, 2);
        await SetBookInventoryAsync(admin, unpopular.Id, branch.Id, 2);

        var (_, memberA) = await RegisterMemberWithProfileAsync(branch.Id);
        var (_, memberB) = await RegisterMemberWithProfileAsync(branch.Id);
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(memberA.Id, popular.Id, branch.Id));
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(memberB.Id, popular.Id, branch.Id));
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(memberA.Id, unpopular.Id, branch.Id));

        var response = await admin.GetAsync($"/api/reports/most-borrowed-books?BranchId={branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<List<MostBorrowedBookDto>>();
        Assert.Equal(2, books!.Count);
        Assert.Equal(popular.Id, books[0].BookId);
        Assert.Equal(2, books[0].BorrowCount);
        Assert.Equal(unpopular.Id, books[1].BookId);
        Assert.Equal(1, books[1].BorrowCount);
    }

    [Fact]
    public async Task GetBranchInventoryReport_ReflectsTotalsForBranch()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var book1 = await CreateBookAsync(admin);
        var book2 = await CreateBookAsync(admin);
        await SetBookInventoryAsync(admin, book1.Id, branch.Id, 4);
        await SetBookInventoryAsync(admin, book2.Id, branch.Id, 6);

        var librarian = await CreateLibrarianClientAsync();
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book1.Id, branch.Id));

        var response = await admin.GetAsync($"/api/reports/branch-inventory?BranchId={branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summaries = await response.Content.ReadFromJsonAsync<List<BranchInventorySummaryDto>>();
        var summary = Assert.Single(summaries!);
        Assert.Equal(branch.Id, summary.BranchId);
        Assert.Equal(2, summary.TotalTitles);
        Assert.Equal(10, summary.TotalCopies);
        Assert.Equal(9, summary.AvailableCopies);
        Assert.Equal(10.0, summary.UtilizationPercentage);
    }

    [Fact]
    public async Task GetMemberActivityReport_ReflectsLoanAndReservationCounts_ScopedToBranch()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, member) = await RegisterMemberWithProfileAsync(branch.Id);
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(member.Id, book.Id, branch.Id));

        var otherBook = await CreateBookAsync(admin);
        await SetBookInventoryAsync(admin, otherBook.Id, branch.Id, 0);
        await librarian.PostAsJsonAsync(
            "/api/reservations", new CreateReservationCommand(member.Id, otherBook.Id, branch.Id));

        var response = await admin.GetAsync($"/api/reports/member-activity?BranchId={branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<MemberActivityDto>>();
        var dto = Assert.Single(page!.Items, m => m.MemberId == member.Id);
        Assert.Equal(1, dto.ActiveLoanCount);
        Assert.Equal(1, dto.TotalLoanCount);
        Assert.Equal(0, dto.OverdueLoanCount);
        Assert.Equal(1, dto.ActiveReservationCount);
    }

    [Fact]
    public async Task GetReservationQueueSummaryReport_ReflectsPendingCountAndReadyFlag_ScopedToBranch()
    {
        var admin = await CreateAdminClientAsync();
        var librarian = await CreateLibrarianClientAsync();
        var (branch, book) = await CreateBookWithInventoryAsync(admin, totalCopies: 1);
        var (_, borrower) = await RegisterMemberWithProfileAsync(branch.Id);
        await librarian.PostAsJsonAsync("/api/loans", new BorrowBookCommand(borrower.Id, book.Id, branch.Id));

        var (_, waiting1) = await RegisterMemberWithProfileAsync(branch.Id);
        var (_, waiting2) = await RegisterMemberWithProfileAsync(branch.Id);
        await librarian.PostAsJsonAsync("/api/reservations", new CreateReservationCommand(waiting1.Id, book.Id, branch.Id));
        await librarian.PostAsJsonAsync("/api/reservations", new CreateReservationCommand(waiting2.Id, book.Id, branch.Id));

        var response = await admin.GetAsync($"/api/reports/reservation-queues?BranchId={branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summaries = await response.Content.ReadFromJsonAsync<List<ReservationQueueSummaryDto>>();
        var summary = Assert.Single(summaries!);
        Assert.Equal(book.Id, summary.BookId);
        Assert.Equal(2, summary.PendingCount);
        Assert.False(summary.HasReadyCopy);
        Assert.NotNull(summary.OldestPendingSinceUtc);
    }

    [Fact]
    public async Task Reports_AsMember_ReturnsForbidden()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var (auth, _) = await RegisterMemberWithProfileAsync(branch.Id);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.GetAsync("/api/reports/overdue-loans");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
