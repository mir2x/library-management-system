using LibraryManagementApi.Application.Reservations.Queries.GetReservationById;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Entities;

namespace LibraryManagementApi.Application.UnitTests.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly GetReservationByIdQueryHandler _handler;

    public GetReservationByIdQueryHandlerTests()
    {
        _handler = new GetReservationByIdQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_WithExistingReservation_ReturnsDtoWithJoinedNamesAndQueuePosition()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        var book = Book.Create("Clean Code", "Robert C. Martin", "9780132350884", "Software Engineering", 2008, null);
        _context.Branches.Add(branch);
        _context.Books.Add(book);
        await _context.SaveChangesAsync(CancellationToken.None);

        var firstInLine = Member.Create("MEM-00000001", "Jane Doe", "jane.doe@example.com", null, null, branch.Id, null);
        var secondInLine = Member.Create("MEM-00000002", "John Smith", "john.smith@example.com", null, null, branch.Id, null);
        _context.Members.AddRange(firstInLine, secondInLine);
        await _context.SaveChangesAsync(CancellationToken.None);

        var first = Reservation.Create(firstInLine.Id, book.Id, branch.Id);
        _context.Reservations.Add(first);
        await _context.SaveChangesAsync(CancellationToken.None);

        var second = Reservation.Create(secondInLine.Id, book.Id, branch.Id);
        _context.Reservations.Add(second);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetReservationByIdQuery(second.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(secondInLine.FullName, result!.MemberName);
        Assert.Equal(book.Title, result.BookTitle);
        Assert.Equal(branch.Name, result.BranchName);
        Assert.Equal(2, result.QueuePosition);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNull()
    {
        var result = await _handler.Handle(new GetReservationByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }
}
