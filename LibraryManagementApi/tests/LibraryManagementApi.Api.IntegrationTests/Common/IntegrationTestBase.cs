using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryManagementApi.Application.Auth;
using LibraryManagementApi.Application.Auth.Commands.Register;
using LibraryManagementApi.Application.Books;
using LibraryManagementApi.Application.Books.Commands.CreateBook;
using LibraryManagementApi.Application.Branches;
using LibraryManagementApi.Application.Branches.Commands.CreateBranch;
using LibraryManagementApi.Application.Members;
using LibraryManagementApi.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementApi.Api.IntegrationTests.Common;

// Concrete test classes must still apply [Collection(IntegrationTestCollection.Name)]
// themselves — xUnit does not reliably discover it via inheritance from this base.
public abstract class IntegrationTestBase(IntegrationTestWebApplicationFactory factory)
{
    protected IntegrationTestWebApplicationFactory Factory { get; } = factory;

    protected HttpClient CreateClient() => Factory.CreateClient();

    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed ({response.StatusCode}): {body}");
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        return client;
    }

    protected Task<HttpClient> CreateAdminClientAsync() =>
        CreateAuthenticatedClientAsync(IntegrationTestWebApplicationFactory.AdminEmail, IntegrationTestWebApplicationFactory.SeedPassword);

    protected Task<HttpClient> CreateLibrarianClientAsync() =>
        CreateAuthenticatedClientAsync(IntegrationTestWebApplicationFactory.LibrarianEmail, IntegrationTestWebApplicationFactory.SeedPassword);

    protected static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@example.com";

    protected static string UniqueIsbn() => string.Concat(Enumerable.Range(0, 13).Select(_ => Random.Shared.Next(0, 10)));

    protected async Task<BranchDto> CreateBranchAsync(HttpClient adminClient, string? name = null)
    {
        var command = new CreateBranchCommand(name ?? UniqueEmail("branch"), "123 Main St", null, null);
        var response = await adminClient.PostAsJsonAsync("/api/branches", command);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<BranchDto>())!;
    }

    protected async Task<AuthResponse> RegisterMemberAsync(HttpClient client, Guid branchId, string? email = null)
    {
        var command = new RegisterCommand(email ?? UniqueEmail("member"), "Password1!", "Test Member", branchId);
        var response = await client.PostAsJsonAsync("/api/auth/register", command);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed ({response.StatusCode}): {body}");
        }

        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    protected async Task<BookDto> CreateBookAsync(HttpClient adminClient, string? title = null)
    {
        var command = new CreateBookCommand(
            title ?? UniqueEmail("book"), "Test Author", UniqueIsbn(), "Fiction", 2020, null);
        var response = await adminClient.PostAsJsonAsync("/api/books", command);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"CreateBook failed ({response.StatusCode}): {body}");
        }

        return (await response.Content.ReadFromJsonAsync<BookDto>())!;
    }

    protected async Task<BookInventoryDto> SetBookInventoryAsync(HttpClient adminClient, Guid bookId, Guid branchId, int totalCopies)
    {
        var response = await adminClient.PutAsJsonAsync($"/api/books/{bookId}/inventory/{branchId}", new { TotalCopies = totalCopies });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"SetBookInventory failed ({response.StatusCode}): {body}");
        }

        return (await response.Content.ReadFromJsonAsync<BookInventoryDto>())!;
    }

    protected async Task<(BranchDto Branch, BookDto Book)> CreateBookWithInventoryAsync(HttpClient adminClient, int totalCopies = 1)
    {
        var branch = await CreateBranchAsync(adminClient);
        var book = await CreateBookAsync(adminClient);
        await SetBookInventoryAsync(adminClient, book.Id, branch.Id, totalCopies);

        return (branch, book);
    }

    protected async Task<(AuthResponse Auth, MemberDto Profile)> RegisterMemberWithProfileAsync(Guid branchId, string? email = null)
    {
        var auth = await RegisterMemberAsync(CreateClient(), branchId, email);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var profile = await client.GetFromJsonWithEnumsAsync<MemberDto>("/api/members/me");

        return (auth, profile!);
    }

    // Backdating a persisted timestamp (e.g. Loan.DueDateUtc) to simulate time passage has no
    // HTTP-level equivalent, so reports whose behaviour depends on "time has passed" (overdue
    // loans, borrow-date ranges) need direct DbContext access to set up that state.
    protected async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }
}
