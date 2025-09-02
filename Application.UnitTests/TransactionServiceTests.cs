using Application.UnitTests.Testing;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Application.Services;
using BudgetTracker.Application.Validation;
using BudgetTracker.Domain.Models;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests
{
    public class TransactionServiceTests
    {
        private static TransactionService Svc(TestRepositoryContext ctx)
        {
            return new TransactionService(ctx, new CreateTransactionRequestValidator(), new TransactionQueryValidator(), TestData.NullLog<TransactionService>());
        }
        [Fact]
        public async Task CreateAsync_throws_when_account_not_found_for_user()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var act = () => Svc(ctx).CreateAsync("u1", new CreateTransactionRequestDto
            {
                AccountId = Guid.NewGuid(),
                Type = TransactionType.Expense,
                Amount = 10,
                Currency = "USD"
            }, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*account not FOUND*");
        }
        [Fact]
        public async Task CreateAsync_throws_when_currency_mismatch()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var acc = TestData.Account("u1", "Wallet", "EUR");
            ctx.Accounts.Add(acc);
            await ctx.SaveChangesAsync();

            var act = () => Svc(ctx).CreateAsync("u1", new CreateTransactionRequestDto
            {
                AccountId = acc.Guid,
                Type = TransactionType.Expense,
                Amount = 10, Currency = "USD" 
            }, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Currency does not match*");
        }
        [Fact]
        public async Task CreateAsync_throws_when_category_not_found_or_archived()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var acc = TestData.Account("u1", "Wallet", "USD");
            ctx.Accounts.Add(acc);
            await ctx.SaveChangesAsync();

            var act = () => Svc(ctx).CreateAsync("u1", new CreateTransactionRequestDto { AccountId = acc.Guid, Type = TransactionType.Expense, Amount = 10, Currency = "USD", CategoryId = Guid.NewGuid() }, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Category Was not found*");
        }

        [Fact]
        public async Task CreateAsync_throws_when_category_type_mismatch()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var acc = TestData.Account("u1", "Wallet", "USD");
            var catIncome = TestData.Category("u1", "Salary", CategoryType.Income);
            ctx.AddRange(acc, catIncome);
            await ctx.SaveChangesAsync();

            var act = () => Svc(ctx).CreateAsync("u1", new CreateTransactionRequestDto { AccountId = acc.Guid, Type = TransactionType.Expense, Amount = 10, Currency = "USD", CategoryId = catIncome.Id }, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Category type must match*");
        }

        [Fact]
        public async Task CreateAsync_success_saves_and_maps_dto_and_trims_note()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var acc = TestData.Account("u1", "Wallet", "USD");
            var cat = TestData.Category("u1", "Groceries", CategoryType.Expense);
            ctx.AddRange(acc, cat);
            await ctx.SaveChangesAsync();

            var before = DateTime.UtcNow;
            var dto = await Svc(ctx).CreateAsync("u1", new CreateTransactionRequestDto
            {
                AccountId = acc.Guid,
                Type = TransactionType.Expense,
                Amount = 42.5m,
                Currency = "usd",
                CategoryId = cat.Id,
                OccurredAtUtc = null, // should default to now
                Note = "  hello  "
            }, CancellationToken.None);
            var after = DateTime.UtcNow;

            dto.Id.Should().NotBeEmpty();
            dto.AccountId.Should().Be(acc.Guid);
            dto.TransactionType.Should().Be(TransactionType.Expense);
            dto.Amount.Should().Be(42.5m);
            dto.Currency.Should().Be("USD");
            dto.CategoryId.Should().Be(cat.Id);
            dto.Note.Should().Be("hello");
            dto.OccurredAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

            (await ctx.Transactions.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task QueryAsync_applies_filters_sorting_and_pagination()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var u1 = "u1"; var u2 = "u2";
            var acc1 = TestData.Account(u1, "A1", "USD");
            var acc2 = TestData.Account(u1, "A2", "USD");
            var accOther = TestData.Account(u2, "O", "USD");
            var catExp = TestData.Category(u1, "Groceries", CategoryType.Expense);
            var catInc = TestData.Category(u1, "Salary", CategoryType.Income);
            ctx.AddRange(acc1, acc2, accOther, catExp, catInc);
            await ctx.SaveChangesAsync();

            var now = DateTime.UtcNow;
            ctx.AddRange(
                TestData.Tx(u1, acc1.Guid, TransactionType.Expense, 10, "USD", now.AddDays(-1), catExp.Id, createdAtUtc: now.AddDays(-1).AddMinutes(1)),
                TestData.Tx(u1, acc1.Guid, TransactionType.Income, 100, "USD", now.AddDays(-2), catInc.Id, createdAtUtc: now.AddDays(-2)),
                TestData.Tx(u1, acc2.Guid, TransactionType.Expense, 5, "USD", now.AddDays(-1), catExp.Id, createdAtUtc: now.AddDays(-1).AddMinutes(2)),
                TestData.Tx(u2, accOther.Guid, TransactionType.Expense, 7, "USD", now, catExp.Id) // other user
            );
            await ctx.SaveChangesAsync();

            var sut = Svc(ctx);
            var page1 = await sut.QueryAsync(u1, new TransactionQuery { FromUtc = now.AddDays(-2), ToUtc = now, Type = null, AccountId = acc1.Guid, Page = 1, PageSize = 1 }, CancellationToken.None);

            page1.TotalCount.Should().Be(2);
            page1.Items.Should().HaveCount(1);
            page1.Items[0].Amount.Should().Be(10);

            var page2 = await sut.QueryAsync(u1, new TransactionQuery { FromUtc = now.AddDays(-2), ToUtc = now, AccountId = acc1.Guid, Page = 2, PageSize = 1 }, CancellationToken.None);
            page2.Items.Should().HaveCount(1);
            page2.Items[0].Amount.Should().Be(100);
        }
    }
}
