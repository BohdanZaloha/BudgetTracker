using BudgetTracker.Application.DTOS;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequestDto>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
            RuleFor(x => x.Note).MaximumLength(500);
        }
    }
}
