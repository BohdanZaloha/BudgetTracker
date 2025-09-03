using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Domain.Models
{
    public class Account
    {
        [Key]
        public Guid Guid { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        [Required, MaxLength(120)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(3)]
        [Column(TypeName = "char(3)")]
        public string Currency { get; set; } = "UAH";

        public bool IsArchived { get; set; } = false;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
