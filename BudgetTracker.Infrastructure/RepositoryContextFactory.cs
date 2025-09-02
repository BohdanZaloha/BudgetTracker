using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BudgetTracker.Infrastructure
{
    internal class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext>
    {
        public RepositoryContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            var cs = configuration.GetConnectionString("Default")
                     ?? "Server=SAINTWHISKAS;Database=BudgetTracker;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<RepositoryContext>()
                .UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(RepositoryContext).Assembly.FullName));

            return new RepositoryContext(optionsBuilder.Options);
        }
    }
}
