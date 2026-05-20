using FinTracker.Application.DTOs;
using FluentValidation;

namespace FinTracker.Application.Validators;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Color).NotEmpty().Matches(@"^#[0-9A-Fa-f]{6}$");
        RuleFor(x => x.Icon).MaximumLength(50).When(x => x.Icon != null);
    }
}
