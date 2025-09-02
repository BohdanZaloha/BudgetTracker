using BudgetTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Application.Abstractions
{
    public interface IRepositoryContext
    {
        DbSet<Account> Accounts { get; }
        DbSet<Category> Categories { get; }
        DbSet<Transaction> Transactions { get; }

        Task<int> SaveChangesAsync(CancellationToken token);

    }
}