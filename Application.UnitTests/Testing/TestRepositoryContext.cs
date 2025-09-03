using BudgetTracker.Application.Abstractions;
using BudgetTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Testing
{
    internal class TestRepositoryContext : DbContext, IRepositoryContext
    {
        public TestRepositoryContext(DbContextOptions<TestRepositoryContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Transaction> Transactions { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public static TestRepositoryContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<TestRepositoryContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging().Options;

            var ctx = new TestRepositoryContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }
    }
}
