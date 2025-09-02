using BudgetTracker.Application.DTOS;

namespace BudgetTracker.Application.Services
{
    public interface IAccountService
    {
        Task<AccountDto> CreateAsync(string userId, CreateAccountRequestDto requestDto, CancellationToken token);
        Task<IReadOnlyList<AccountDto>> GetAllAsync(string userId, CancellationToken token);
    }
}
