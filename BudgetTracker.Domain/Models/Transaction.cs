using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Domain.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        [Required]
        public Guid AccountId { get; set; }
        public Account Account { get; set; } = default!;

        [Required]
        public TransactionType Type { get; set; }            // 0=Expense, 1=Income

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be > 0")]
        public decimal Amount { get; set; }

        [Required, MaxLength(3)]
        [Column(TypeName = "char(3)")]
        public string Currency { get; set; } = "UAH";        // для MVP = валюті акаунта (перевір в сервісі)

        public Guid? CategoryId { get; set; }                // опційно
        public Category? Category { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool IsDeleted { get; set; } = false;         // глобальний фільтр у DbContext

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
