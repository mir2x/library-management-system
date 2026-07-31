using LibraryManagementApi.Application.Reservations.Queries.GetMyReservations;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetMyReservationsQueryHandler _handler;

    public GetMyReservationsQueryHandlerTests()
    {
        _handler = new GetMyReservationsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithLinkedMemberAndReservations_ReturnsOnlyThatMembersReservations()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var me = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, userId: "user-1");
        var someoneElse = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, userId: "user-2");
        _context.Members.AddRange(me, someoneElse);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Reservations.AddRange(
            Reservation.Create(me.Id, book.Id, branch.Id),
            Reservation.Create(someoneElse.Id, book.Id, branch.Id));
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetMyReservationsQuery("user-1"), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(me.FullName, result.Items[0].MemberName);
    }

    [Fact]
    public async Task Handle_WithNoLinkedMember_ReturnsEmptyPage()
    {
        var result = await _handler.Handle(new GetMyReservationsQuery("user-without-member"), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
