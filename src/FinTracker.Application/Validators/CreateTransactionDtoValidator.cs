using FinTracker.Application.DTOs;
using FinTracker.Application.Helpers;
using FluentValidation;

namespace FinTracker.Application.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(_ => UtcDateHelper.ToUtcDate(DateTime.UtcNow).AddDays(1));
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note != null);
    }
}
