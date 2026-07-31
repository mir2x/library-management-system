using LibraryManagementApi.Application.Auth.Commands.ResetPassword;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Models;
using NSubstitute;

namespace LibraryManagementApi.Application.UnitTests.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _handler = new ResetPasswordCommandHandler(_identityService);
    }

    [Fact]
    public async Task Handle_DelegatesToIdentityServiceAndReturnsSuccess()
    {
        var command = new ResetPasswordCommand("jane.doe@example.com", "reset-token", "Str0ngPass!");

        _identityService
            .ResetPasswordAsync(command.Email, command.Token, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceFails_ReturnsFailure()
    {
        var command = new ResetPasswordCommand("jane.doe@example.com", "invalid-token", "Str0ngPass!");
        string[] errors = ["Invalid or expired reset token."];

        _identityService
            .ResetPasswordAsync(command.Email, command.Token, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(errors));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(errors, result.Errors);
    }
}
