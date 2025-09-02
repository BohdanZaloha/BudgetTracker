namespace BudgetTracker.Application.DTOS
{
    public class CreateAccountRequestDto
    {
        public string Name { get; init; } = default!;
        public string Currency { get; init; } = default!;
    }
}
