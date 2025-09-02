namespace BudgetTracker.Application.Dtos
{
    public class RegisterUserCommand
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
        public string? UserName { get; init; }
    }
}
