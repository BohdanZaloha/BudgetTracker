using BudgetTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Infrastructure.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> b)
        {
            b.ToTable("Transactions");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();

            b.Property(x => x.Type).HasConversion<byte>().HasColumnType("tinyint");


            b.Property(x => x.OccurredAtUtc).HasColumnType("datetime2").IsRequired();

            b.Property(x => x.IsDeleted).HasDefaultValue(false);

            b.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

            b.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

            b.HasOne(x => x.User).WithMany(u => u.Transactions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.Account).WithMany(a => a.Transactions).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Category).WithMany(c => c.Transactions).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);

            b.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Transactions_Type", "[Type] IN (0,1)");
                t.HasCheckConstraint("CK_Transactions_Amount_Positive", "[Amount] > 0");
            });
        }
    }
}
