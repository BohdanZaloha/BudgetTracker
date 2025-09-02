using BudgetTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Type).HasConversion<byte>().HasColumnType("tinyint");

            builder.Property(x => x.IsArchived).HasDefaultValue(false);

            builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

            // Self-FK for hierarchy (SetNull keeps children when parent removed)
            builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.SetNull); //??

            // Unique (UserId, Name, Type)
            builder.HasIndex(x => new { x.UserId, x.Name, x.Type }).IsUnique();

            // FK to AspNetUsers (no cascade)
            builder.HasOne(x => x.User).WithMany(u => u.Categories).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);

        }
    }
}
