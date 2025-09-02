using Application.UnitTests.Testing;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Application.Services;
using BudgetTracker.Application.Validation;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests
{
    public class AccountServiceTests
    {
        [Fact]
        public async Task CreateAsync_creates_account_and_maps_dto()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var sut = new AccountService(ctx, new CreateAccountRequestValidator(), TestData.NullLog<AccountService>());
            var requestDto = new CreateAccountRequestDto { Name = "Wallet", Currency = "USD" };
            var result = await sut.CreateAsync("user1", requestDto, CancellationToken.None);

            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be("Wallet");
            result.Currency.Should().Be("USD");

            (await ctx.Accounts.CountAsync()).Should().Be(1);
            var saved = await ctx.Accounts.SingleAsync();
            saved.UserId.Should().Be("user1");
            saved.Name.Should().Be("Wallet");
            saved.Currency.Should().Be("USD");
        }
        [Fact]
        public async Task CreateAsync_when_duplicate_name_for_user_throws_validation_exception()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            ctx.Accounts.Add(TestData.Account("user1", "Wallet", "USD"));
            await ctx.SaveChangesAsync();

            var sut = new AccountService(ctx, new CreateAccountRequestValidator(), TestData.NullLog<AccountService>());
            var act = () => sut.CreateAsync("user1", new CreateAccountRequestDto { Name = "Wallet", Currency = "USD" }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*AccountWithTheSameNameExists*");
        }
        [Fact]
        public async Task GetAllAsync_returns_only_users_nonarchived_sorted_by_name()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var user1 = "user1";
            var user2 = "user2";
            ctx.Accounts.AddRange
                (
                TestData.Account(user1, "bONE", "USD"),
                TestData.Account(user1, "two", "UAH", archived: true),
                TestData.Account(user1, "aONE", "USD"),
                TestData.Account(user2, "ThRee", "EUR")
                );
            await ctx.SaveChangesAsync();
            var sut = new AccountService(ctx, new CreateAccountRequestValidator(), TestData.NullLog<AccountService>());
            var list = await sut.GetAllAsync(user1, CancellationToken.None);

            list.Should().HaveCount(2);
            list.Select(x => x.Name).Should().ContainInOrder("aONE", "bONE");
        }

    }
}
