using BudgetTracker.Application.Dtos;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    internal class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        }
    }
}
