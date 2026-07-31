using FluentValidation;

namespace LibraryManagementApi.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(300)
            .When(x => x.Title is not null);

        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Author is not null);

        RuleFor(x => x.Genre)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Genre is not null);

        RuleFor(x => x.PublishedYear)
            .Must(year => year is null || (year >= 1450 && year <= DateTime.UtcNow.Year))
            .WithMessage($"Published year must be between 1450 and {DateTime.UtcNow.Year}.");

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}
