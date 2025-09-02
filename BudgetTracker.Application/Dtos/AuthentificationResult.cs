namespace BudgetTracker.Application.Dtos
{
    public class AuthentificationResult
    {
        public string AccessToken { get; init; } = default!;
        public int ExpiresInSeconds { get; init; }
        public string UserId { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string UserName { get; init; } = default!;
        public string[] Roles { get; init; } = Array.Empty<string>();
    }
}
