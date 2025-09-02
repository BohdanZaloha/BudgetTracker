
using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
    public enum CategoryType : byte
    {
        Expense = 0,
        Income = 1
    }

    public enum TransactionType : byte
    {
        Expense = 0,
        Income = 1
    }
}
