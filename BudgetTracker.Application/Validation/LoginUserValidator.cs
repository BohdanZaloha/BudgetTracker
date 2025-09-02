using BudgetTracker.Application.Dtos;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    internal class LoginUserValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
