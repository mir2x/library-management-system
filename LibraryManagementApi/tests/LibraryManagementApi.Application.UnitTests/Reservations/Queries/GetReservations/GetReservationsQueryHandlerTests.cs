using LibraryManagementApi.Application.Reservations.Queries.GetReservations;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Queries.GetReservations;

public class GetReservationsQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetReservationsQueryHandler _handler;

    public GetReservationsQueryHandlerTests()
    {
        _handler = new GetReservationsQueryHandler(_context);
    }

    private async Task<(Member Member1, Member Member2, Book Book1, Book Book2, Branch Branch)> SeedAsync()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        var book2 = Book.Create("Refactoring", "Martin Fowler", "9780134757599", "Software Engineering", 2018, null);
        _context.Branches.Add(branch);
        _context.Books.AddRange(book1, book2);
        await _context.SaveChangesAsync(CancellationToken.None);

        var member1 = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        var member2 = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (member1, member2, book1, book2, branch);
    }

    [Fact]
    public async Task Handle_WithMemberIdFilter_ReturnsOnlyThatMembersReservations()
    {
        var (member1, member2, book1, book2, branch) = await SeedAsync();
        _context.Reservations.AddRange(
            Reservation.Create(member1.Id, book1.Id, branch.Id),
            Reservation.Create(member2.Id, book2.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationsQuery(member1.Id, null, null, null), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(member1.FullName, result.Items[0].MemberName);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingReservations()
    {
        var (member1, member2, book1, book2, branch) = await SeedAsync();
        var ready = Reservation.Create(member1.Id, book1.Id, branch.Id);
        ready.MarkReady();
        var pending = Reservation.Create(member2.Id, book2.Id, branch.Id);
        _context.Reservations.AddRange(ready, pending);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationsQuery(null, null, null, ReservationStatus.Ready), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(ReservationStatus.Ready, result.Items[0].Status);
    }

    [Fact]
    public async Task Handle_ComputesQueuePositionOnlyForPendingReservations()
    {
        var (member1, member2, book1, _, branch) = await SeedAsync();
        var first = Reservation.Create(member1.Id, book1.Id, branch.Id);
        _context.Reservations.Add(first);
        await _context.SaveChangesAsync(CancellationToken.None);

        var second = Reservation.Create(member2.Id, book1.Id, branch.Id);
        second.MarkReady();
        _context.Reservations.Add(second);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationsQuery(null, book1.Id, null, null), CancellationToken.None);

        var firstDto = result.Items.Single(r => r.MemberId == member1.Id);
        var secondDto = result.Items.Single(r => r.MemberId == member2.Id);

        Assert.Equal(1, firstDto.QueuePosition);
        Assert.Equal(0, secondDto.QueuePosition);
    }
}
