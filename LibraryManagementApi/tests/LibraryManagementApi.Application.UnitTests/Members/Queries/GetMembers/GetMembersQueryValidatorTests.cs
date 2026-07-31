using LibraryManagementApi.Application.Members.Queries.GetMembers;

namespace LibraryManagementApi.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryValidatorTests
{
    private readonly GetMembersQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaultPaging_HasNoErrors()
    {
        var query = new GetMembersQuery(null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_HasError()
    {
        var query = new GetMembersQuery(null, PageNumber: 0);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMembersQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithPageSizeOutOfRange_HasError(int pageSize)
    {
        var query = new GetMembersQuery(null, PageSize: pageSize);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetMembersQuery.PageSize));
    }
}
