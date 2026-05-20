using FinTracker.Application.DTOs;
using FluentValidation;

namespace FinTracker.Application.Validators;

public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
