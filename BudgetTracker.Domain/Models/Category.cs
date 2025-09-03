using BudgetTracker.Domain.Enumerables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Domain.Models
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        [Required, MaxLength(120)]
        public string Name { get; set; } = default!;

        [Required]
        public CategoryType Type { get; set; } // 0=Expense, 1=Income

        public Guid? ParentId { get; set; } 
        public Category? Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();

        public bool IsArchived { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Навігації
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
