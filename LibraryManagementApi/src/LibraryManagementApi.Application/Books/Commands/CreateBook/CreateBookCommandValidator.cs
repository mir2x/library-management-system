using FluentValidation;

namespace LibraryManagementApi.Application.Books.Commands.CreateBook;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Isbn)
            .NotEmpty()
            .Must(IsbnHelper.IsValid)
            .WithMessage("ISBN must be a valid 10 or 13 character ISBN.");

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Published year must be between 1450 and {DateTime.UtcNow.Year}.");

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}
