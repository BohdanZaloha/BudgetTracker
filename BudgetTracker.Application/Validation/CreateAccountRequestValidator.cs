using BudgetTracker.Application.DTOS;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequestDto>
    {
        public CreateAccountRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
        }
    }
}
