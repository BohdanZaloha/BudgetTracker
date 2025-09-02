namespace BudgetTracker.Application.Dtos
{
    public class LoginUserCommand
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}
