using BudgetTracker.Domain.Enumerables;

namespace BudgetTracker.Application.DTOS
{
    public class CategoryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public CategoryType CategoryType { get; init; }
        public Guid? ParentId { get; init; }
        public bool IsArchived { get; init; }
    }
}
