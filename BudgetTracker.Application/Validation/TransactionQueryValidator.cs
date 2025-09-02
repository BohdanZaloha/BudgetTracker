using BudgetTracker.Application.Dtos;
using FluentValidation;

namespace BudgetTracker.Application.Validation
{
    public class TransactionQueryValidator : AbstractValidator<TransactionQuery>
    {
        public TransactionQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
            When(x => x.FromUtc.HasValue && x.ToUtc.HasValue, () =>
            {
                RuleFor(x => x).Must(x => x.FromUtc <= x.ToUtc).WithMessage("FromUtc must be before ToUtc.");
            });
        }
    }
}
