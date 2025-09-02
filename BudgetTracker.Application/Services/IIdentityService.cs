namespace BudgetTracker.Infrastructure.Identity
{
    public interface IIdentityService
    {
        Task<(bool Succeeded, string? UserId, string[] Errors)> CreateUserAsync(string email, string? userName, string password, CancellationToken token);

        Task<(bool Succeeded, string UserId)> ValidateCredentialsAsync(string email, string password, CancellationToken token);

        Task<string[]> GetRolesAsync(string userId, CancellationToken ct);

        Task<(string UserId, string UserName, string? Email)?> FindByEmailAsync(string email, CancellationToken ct);
    }
}
