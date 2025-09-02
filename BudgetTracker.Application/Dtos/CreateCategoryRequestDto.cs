using BudgetTracker.Domain.Models;

namespace BudgetTracker.Application.DTOS
{
    public class CreateCategoryRequestDto
    {
        public string Name { get; init; } = default!;
        public CategoryType Type { get; init; }
        public Guid? ParentId { get; init; }
    }
}
