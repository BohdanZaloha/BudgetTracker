namespace BudgetTracker.Application.DTOS
{
    public class AccountDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Currency { get; init; } = default!;
        public bool IsArchived { get; init; }
    }
}
