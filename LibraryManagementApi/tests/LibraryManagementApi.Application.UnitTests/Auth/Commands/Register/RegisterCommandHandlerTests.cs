using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Register;
using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using LibraryManagementApi.Application.UnitTests.TestSupport;
using LibraryManagementApi.Domain.Constants;
using LibraryManagementApi.Domain.Entities;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly TestApplicationDbContext _context = TestApplicationDbContextFactory.Create();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuthTokenIssuer _authTokenIssuer = Substitute.For<IAuthTokenIssuer>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_identityService, _authTokenIssuer, _context);
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserAsMemberIssuesTokensAndCreatesLinkedMemberProfile()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe", branch.Id);
        var authenticatedUser = new AuthenticatedUser("user-1", command.Email, command.FullName, [Roles.Member]);
        var expectedResponse = new AuthResponse(
            authenticatedUser.Id,
            authenticatedUser.Email,
            authenticatedUser.FullName,
            authenticatedUser.Roles,
            "access-token",
            DateTime.UtcNow.AddMinutes(15),
            "refresh-token",
            DateTime.UtcNow.AddDays(7));

        _identityService
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticatedUser>.Success(authenticatedUser));

        _authTokenIssuer
            .IssueTokensAsync(authenticatedUser, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(expectedResponse, result.Value);
        await _identityService.Received(1)
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>());

        var member = Assert.Single(_context.Members);
        Assert.Equal(authenticatedUser.Id, member.UserId);
        Assert.Equal(branch.Id, member.HomeBranchId);
        Assert.Equal(command.Email, member.Email);
        Assert.StartsWith("MEM-", member.MembershipNumber);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceFails_ReturnsFailureAndDoesNotIssueTokensOrCreateMember()
    {
        var branch = Branch.Create("Downtown Branch", "123 Main St", null, null);
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe", branch.Id);
        string[] errors = ["Email 'jane.doe@example.com' is already taken."];

        _identityService
            .CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Member, Arg.Any<CancellationToken>())
            .Returns(Result<AuthenticatedUser>.Failure(errors));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(errors, result.Errors);
        await _authTokenIssuer.DidNotReceive().IssueTokensAsync(Arg.Any<AuthenticatedUser>(), Arg.Any<CancellationToken>());
        Assert.Empty(_context.Members);
    }

    [Fact]
    public async Task Handle_WithUnknownBranchId_ThrowsNotFoundException()
    {
        var command = new RegisterCommand("jane.doe@example.com", "Str0ngPass!", "Jane Doe", Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        await _identityService.DidNotReceive()
            .CreateUserAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
