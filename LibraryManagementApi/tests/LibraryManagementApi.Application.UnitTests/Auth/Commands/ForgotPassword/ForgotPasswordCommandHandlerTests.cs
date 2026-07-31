using LibraryManagementApi.Application.Auth.Commands.ForgotPassword;
using LibraryManagementApi.Application.Common.Interfaces;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IAppUrlProvider _appUrlProvider = Substitute.For<IAppUrlProvider>();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _handler = new ForgotPasswordCommandHandler(_identityService, _emailSender, _appUrlProvider);
    }

    [Fact]
    public async Task Handle_WithExistingAccount_SendsResetEmailAndReturnsSuccess()
    {
        const string email = "jane.doe@example.com";
        const string token = "reset-token";
        const string resetUrl = "http://localhost:5173/reset-password?email=jane.doe%40example.com&token=reset-token";

        _identityService.GeneratePasswordResetTokenAsync(email, Arg.Any<CancellationToken>()).Returns(token);
        _appUrlProvider.BuildPasswordResetUrl(email, token).Returns(resetUrl);

        var result = await _handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

        Assert.True(result.Succeeded);
        await _emailSender.Received(1).SendAsync(
            email,
            Arg.Any<string>(),
            Arg.Is<string>(body => body != null && body.Contains(resetUrl)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoAccountForEmail_DoesNotSendEmailButStillReturnsSuccess()
    {
        const string email = "unknown@example.com";

        _identityService.GeneratePasswordResetTokenAsync(email, Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

        Assert.True(result.Succeeded);
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
