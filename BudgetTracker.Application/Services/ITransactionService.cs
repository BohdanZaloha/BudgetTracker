using BudgetTracker.Application.Common;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.DTOS;

namespace BudgetTracker.Application.Services
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateAsync(string userId, CreateTransactionRequestDto requestDto, CancellationToken token);

        Task<PagedResult<TransactionDto>> QueryAsync(string userId, TransactionQuery query, CancellationToken token);
    }
}
