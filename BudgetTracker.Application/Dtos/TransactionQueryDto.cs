using BudgetTracker.Domain.Enumerables;

namespace BudgetTracker.Application.Dtos
{
    public class TransactionQuery
    {
        public DateTime? FromUtc { get; init; }
        public DateTime? ToUtc { get; init; }
        public TransactionType? Type { get; init; }
        public Guid? CategoryId { get; init; }
        public Guid? AccountId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
