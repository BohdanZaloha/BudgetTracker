namespace BudgetTracker.Application.Common
{
    /// <summary>
    /// Represents a single page of results.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
