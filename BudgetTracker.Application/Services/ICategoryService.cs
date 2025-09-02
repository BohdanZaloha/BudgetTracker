using BudgetTracker.Application.DTOS;

namespace BudgetTracker.Application.Services
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(string userId, CreateCategoryRequestDto requestDto, CancellationToken token);

        Task<IReadOnlyList<CategoryDto>> GetAllAsync(string userId, CancellationToken token);
    }
}
