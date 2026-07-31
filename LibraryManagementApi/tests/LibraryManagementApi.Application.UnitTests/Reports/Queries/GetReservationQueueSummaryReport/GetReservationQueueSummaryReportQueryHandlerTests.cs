using LibraryManagementApi.Application.Reports.Queries.GetReservationQueueSummaryReport;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reports.Queries.GetReservationQueueSummaryReport;

public class GetReservationQueueSummaryReportQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetReservationQueueSummaryReportQueryHandler _handler;

    public GetReservationQueueSummaryReportQueryHandlerTests()
    {
        _handler = new GetReservationQueueSummaryReportQueryHandler(_context);
    }

    private async Task<(Member Member1, Member Member2, Book Book, Branch Branch)> SeedAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member1 = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        var member2 = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member1, member2, book, branch);
    }

    [Fact]
    public async Task Handle_GroupsByBookAndBranch_CountsPendingAndFlagsReady()
    {
        var (member1, member2, book, branch) = await SeedAsync();
        var pending = Reservation.Create(member1.Id, book.Id, branch.Id);
        var ready = Reservation.Create(member2.Id, book.Id, branch.Id);
        ready.MarkReady();
        _context.Reservations.AddRange(pending, ready);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationQueueSummaryReportQuery(null), CancellationToken.None);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(book.Title, dto.BookTitle);
        Assert.Equal(branch.Name, dto.BranchName);
        Assert.Equal(1, dto.PendingCount);
        Assert.True(dto.HasReadyCopy);
        Assert.NotNull(dto.OldestPendingSinceUtc);
    }

    [Fact]
    public async Task Handle_ExcludesFulfilledAndCancelledReservations()
    {
        var (member1, member2, book, branch) = await SeedAsync();
        var fulfilled = Reservation.Create(member1.Id, book.Id, branch.Id);
        fulfilled.MarkReady();
        fulfilled.MarkFulfilled();
        var cancelled = Reservation.Create(member2.Id, book.Id, branch.Id);
        cancelled.Cancel();
        _context.Reservations.AddRange(fulfilled, cancelled);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationQueueSummaryReportQuery(null), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithBranchFilter_ExcludesOtherBranches()
    {
        var (member1, _, book, branch) = await SeedAsync();
        _context.Reservations.Add(Reservation.Create(member1.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationQueueSummaryReportQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithOnlyReadyReservation_HasNullOldestPendingSince()
    {
        var (member1, _, book, branch) = await SeedAsync();
        var ready = Reservation.Create(member1.Id, book.Id, branch.Id);
        ready.MarkReady();
        _context.Reservations.Add(ready);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationQueueSummaryReportQuery(null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(0, result[0].PendingCount);
        Assert.True(result[0].HasReadyCopy);
        Assert.Null(result[0].OldestPendingSinceUtc);
    }
}
