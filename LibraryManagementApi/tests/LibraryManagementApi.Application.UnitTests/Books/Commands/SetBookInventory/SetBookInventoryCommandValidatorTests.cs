using LibraryManagementApi.Application.Books.Commands.SetBookInventory;

namespace LibraryManagementApi.Application.UnitTests.Books.Commands.SetBookInventory;

public class SetBookInventoryCommandValidatorTests
{
    private readonly SetBookInventoryCommandValidator _validator = new();

    [Fact]
    public void Validate_WithNonNegativeTotalCopies_HasNoErrors()
    {
        var command = new SetBookInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), 5);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNegativeTotalCopies_HasError()
    {
        var command = new SetBookInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), -1);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SetBookInventoryCommand.TotalCopies));
    }
}
