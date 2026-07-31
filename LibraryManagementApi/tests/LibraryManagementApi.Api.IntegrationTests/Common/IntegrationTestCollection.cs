namespace LibraryManagementApi.Api.IntegrationTests.Common;

// One Postgres container for the whole test run, shared by every class in this collection.
// xUnit runs classes within a collection sequentially, so tests can't race each other over it.
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebApplicationFactory>
{
    public const string Name = "Integration";
}
