using BudgetTracker.Application.DTOS;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Type).IsInEnum();
        }
    }
}
