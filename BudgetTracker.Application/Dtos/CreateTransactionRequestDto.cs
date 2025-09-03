using BudgetTracker.Domain.Enumerables;
using BudgetTracker.Domain.Models;

namespace BudgetTracker.Application.DTOS
{
    public class CreateTransactionRequestDto
    {
        public Guid AccountId { get; init; }
        public TransactionType Type { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = default!;
        public Guid? CategoryId { get; init; }
        public DateTime? OccurredAtUtc { get; init; }
        public string? Note { get; init; }
    }
}
