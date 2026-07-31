using System.Net;
using System.Net.Http.Json;
using LibraryManagementApi.Api.IntegrationTests.Common;
using LibraryManagementApi.Application.Members;
using LibraryManagementApi.Domain.Enums;

namespace LibraryManagementApi.Api.IntegrationTests.Members;

[Collection(IntegrationTestCollection.Name)]
public class MemberEndpointsTests(IntegrationTestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_CreatesLinkedMemberProfile_VisibleViaMyProfile()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var email = UniqueEmail("member");
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id, email);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.GetAsync("/api/members/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<MemberDto>();
        Assert.Equal(email, profile!.Email);
        Assert.Equal(branch.Id, profile.HomeBranchId);
        Assert.StartsWith("MEM-", profile.MembershipNumber);
    }

    [Fact]
    public async Task GetMembers_AsMember_ReturnsForbidden()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var response = await client.GetAsync("/api/members");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMembers_AsLibrarian_CanListMembers()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        await RegisterMemberAsync(CreateClient(), branch.Id);
        var librarian = await CreateLibrarianClientAsync();

        var response = await librarian.GetAsync("/api/members?PageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<MemberDto>>();
        Assert.NotEmpty(page!.Items);
    }

    [Fact]
    public async Task SuspendMember_ThenReactivate_TogglesStatus()
    {
        var admin = await CreateAdminClientAsync();
        var branch = await CreateBranchAsync(admin);
        var auth = await RegisterMemberAsync(CreateClient(), branch.Id);

        var meResponse = await GetMyProfileAsync(auth.AccessToken);
        var memberId = meResponse.Id;

        var suspendResponse = await admin.PostAsync($"/api/members/{memberId}/suspend", null);
        Assert.Equal(HttpStatusCode.NoContent, suspendResponse.StatusCode);

        var afterSuspend = await admin.GetFromJsonAsync<MemberDto>($"/api/members/{memberId}");
        Assert.Equal(MembershipStatus.Suspended, afterSuspend!.Status);

        var reactivateResponse = await admin.PostAsync($"/api/members/{memberId}/reactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var afterReactivate = await admin.GetFromJsonAsync<MemberDto>($"/api/members/{memberId}");
        Assert.Equal(MembershipStatus.Active, afterReactivate!.Status);
    }

    private async Task<MemberDto> GetMyProfileAsync(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

        return (await client.GetFromJsonAsync<MemberDto>("/api/members/me"))!;
    }
}
