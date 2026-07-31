using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Register;

namespace LibraryManagementApi.Api.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public class AuthEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var client = CreateClient();

        var command = new RegisterCommand(UniqueEmail("member"), "Password1!", "Jane Doe", branch.Id);
        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(command.Email, auth!.Email);
        Assert.Contains("Member", auth.Roles);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
    }

    [Fact]
    public async Task Register_WithUnknownBranch_ReturnsNotFound()
    {
        var client = CreateClient();

        var command = new RegisterCommand(UniqueEmail("member"), "Password1!", "Jane Doe", Guid.NewGuid());
        var response = await client.PostAsJsonAsync("/api/auth/register", command);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var email = UniqueEmail("member");
        await RegisterMemberAsync(CreateClient(), branch.Id, email);

        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var email = UniqueEmail("member");
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id, email);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.Equal(email, me!.Email);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewTokenPair()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id);

        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotEqual(auth.AccessToken, refreshed!.AccessToken);
        Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_SoSubsequentRefreshFails()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id);

        var client = CreateClient();
        var logoutResponse = await client.PostAsJsonAsync("/api/auth/logout", new { RefreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new { RefreshToken = auth.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_AlwaysReturnsNoContent_RegardlessOfWhetherAccountExists()
    {
        var client = CreateClient();

        var existingResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new { Email = IntegrationTestWebApplicationFactory.AdminEmail });
        var unknownResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new { Email = UniqueEmail("nobody") });

        Assert.Equal(HttpStatusCode.NoContent, existingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, unknownResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithTokenFromEmail_AllowsLoginWithNewPassword()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var email = UniqueEmail("member");
        await RegisterMemberAsync(CreateClient(), branch.Id, email);

        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/forgot-password", new { Email = email });

        var sentEmail = Factory.EmailSender.SentEmails.Last(e => e.ToEmail == email);
        var token = Uri.UnescapeDataString(Regex.Match(sentEmail.HtmlBody, "token=([^&\"]+)").Groups[1].Value);

        var resetResponse = await client.PostAsJsonAsync(
            "/api/auth/reset-password", new { Email = email, Token = token, NewPassword = "NewPassword1!" });
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "NewPassword1!" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }
}
