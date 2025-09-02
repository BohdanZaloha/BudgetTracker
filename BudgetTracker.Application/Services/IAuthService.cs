using BudgetTracker.Application.Dtos;

namespace BudgetTracker.Application.Services
{
    public interface IAuthService
    {
        Task<AuthentificationResult> RegisterAsync(RegisterUserCommand cmd, CancellationToken token);
        Task<AuthentificationResult> LoginAsync(LoginUserCommand cmd, CancellationToken token);
    }
}
