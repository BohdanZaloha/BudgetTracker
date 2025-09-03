using BudgetTracker.Domain.Enumerables;

namespace BudgetTracker.Application.DTOS
{
    public class TransactionDto
    {
        public Guid Id { get; init; }
        public Guid AccountId { get; init; }
        public TransactionType TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = default!;
        public Guid? CategoryId { get; init; }
        public DateTime OccurredAtUtc { get; init; }
        public string? Note { get; init; }
    }
}
