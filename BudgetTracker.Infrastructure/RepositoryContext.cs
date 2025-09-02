using BudgetTracker.Application.Abstractions;
using BudgetTracker.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure
{
    public class RepositoryContext : IdentityDbContext<ApplicationUser>, IRepositoryContext
    {
        public RepositoryContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RepositoryContext).Assembly);

            //// Global filters
            //modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
        }
    }

}
