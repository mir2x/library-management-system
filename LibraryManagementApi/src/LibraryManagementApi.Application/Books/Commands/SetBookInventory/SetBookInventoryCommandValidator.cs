using FluentValidation;

namespace LibraryManagementApi.Application.Books.Commands.SetBookInventory;

public class SetBookInventoryCommandValidator : AbstractValidator<SetBookInventoryCommand>
{
    public SetBookInventoryCommandValidator()
    {
        // Input-shape validation only. The stateful rule (can't drop total copies below the
        // number currently borrowed) needs the current inventory row, so it's enforced by
        // BookInventory itself (see DomainException), not here.
        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(0);
    }
}
