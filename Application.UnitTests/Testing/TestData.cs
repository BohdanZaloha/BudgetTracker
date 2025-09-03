using BudgetTracker.Domain.Enumerables;
using BudgetTracker.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.UnitTests.Testing
{
    internal static class TestData
    {
        public static Account Account(string userId, string name, string currency = "USD", bool archived = false, Guid? id = null)
        {
            return new Account { Guid = id ?? Guid.NewGuid(), UserId = userId, Name = name, Currency = currency, IsArchived = archived };
        }


        public static Category Category(string userId, string name, CategoryType type, Guid? id = null, Guid? parentId = null, bool archived = false)
        {
            return new Category { Id = id ?? Guid.NewGuid(), UserId = userId, Name = name, Type = type, ParentId = parentId, IsArchived = archived };
        }


        public static Transaction Tx(string userId, Guid accountId, TransactionType type, decimal amount, string currency, DateTime occurredAtUtc, Guid? categoryId = null, string? note = null, bool deleted = false, DateTime? createdAtUtc = null)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = accountId,
                Type = type,
                Amount = amount,
                Currency = currency,
                CategoryId = categoryId,
                OccurredAtUtc = occurredAtUtc,
                Note = note,
                IsDeleted = deleted,
                CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
            };
        }

        public static NullLogger<T> NullLog<T>()
        {
            return NullLogger<T>.Instance;
        }
    }
}
