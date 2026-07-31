using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Branches;
using LibraryManagementApi.Application.Branches.Commands.CreateBranch;

namespace LibraryManagementApi.Api.IntegrationTests.Branches;

[Collection(IntegrationTestCollection.Name)]
public class BranchEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateBranch_AsAdmin_ReturnsCreated()
    {
        var admin = await CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync(
            "/api/branches", new CreateBranchCommand(UniqueEmail("branch"), "123 Main St", "555-0100", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBranch_AsLibrarian_ReturnsForbidden()
    {
        var librarian = await CreateLibrarianClientAsync();

        var response = await librarian.PostAsJsonAsync(
            "/api/branches", new CreateBranchCommand(UniqueEmail("branch"), "123 Main St", null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBranch_Unauthenticated_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/branches", new CreateBranchCommand(UniqueEmail("branch"), "123 Main St", null, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBranchById_ReturnsCreatedBranch()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);

        var response = await admin.GetAsync($"/api/branches/{branch.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<BranchDto>();
        Assert.Equal(branch.Name, dto!.Name);
    }

    [Fact]
    public async Task GetBranches_WithSearch_FiltersByName()
    {
        var admin = await CreateAdminClientAsync();
        var uniqueName = UniqueEmail("searchable-branch");
        await CreateBranchAsync(admin, uniqueName);
        await CreateBranchAsync(admin);

        var response = await admin.GetAsync($"/api/branches?Search={Uri.EscapeDataString(uniqueName)}&PageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<BranchDto>>();
        Assert.Single(page!.Items);
        Assert.Equal(uniqueName, page.Items[0].Name);
    }

    [Fact]
    public async Task UpdateBranch_PatchOnlyProvidedFields_LeavesOthersUnchanged()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);

        var patchResponse = await admin.PatchAsJsonAsync(
            $"/api/branches/{branch.Id}", new { Name = (string?)null, Address = "456 New Address", ContactNumber = (string?)null, Email = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var getResponse = await admin.GetAsync($"/api/branches/{branch.Id}");
        var dto = await getResponse.Content.ReadFromJsonAsync<BranchDto>();
        Assert.Equal(branch.Name, dto!.Name);
        Assert.Equal("456 New Address", dto.Address);
    }

    [Fact]
    public async Task DeleteBranch_SoftDeletes_ExcludesFromListButNotFromGetById()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);

        var deleteResponse = await admin.DeleteAsync($"/api/branches/{branch.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await admin.GetAsync($"/api/branches/{branch.Id}");
        var dto = await getResponse.Content.ReadFromJsonAsync<BranchDto>();
        Assert.False(dto!.IsActive);

        var listResponse = await admin.GetAsync($"/api/branches?Search={Uri.EscapeDataString(branch.Name)}");
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResponse<BranchDto>>();
        Assert.Empty(page!.Items);
    }
}
