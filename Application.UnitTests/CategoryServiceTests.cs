using Application.UnitTests.Testing;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Application.Services;
using BudgetTracker.Application.Validation;
using BudgetTracker.Domain.Enumerables;
using FluentAssertions;
using FluentValidation;

namespace Application.UnitTests
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task CreateAsync_success_maps_dto()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var req = new CreateCategoryRequestDto { Name = "Food", Type = CategoryType.Expense };

            var dto = await sut.CreateAsync("user1", req, CancellationToken.None);

            dto.Id.Should().NotBeEmpty();
            dto.Name.Should().Be("Food");
            dto.CategoryType.Should().Be(CategoryType.Expense);
            dto.ParentId.Should().BeNull();
            dto.IsArchived.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_with_parent_of_different_type_throws_validation_exception()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var parent = TestData.Category("user1", "Salary", CategoryType.Income);
            ctx.Categories.Add(parent);
            await ctx.SaveChangesAsync();

            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var req = new CreateCategoryRequestDto
            {
                Name = "Groceries",
                Type = CategoryType.Expense,
                ParentId = parent.Id
            };

            var act = () => sut.CreateAsync("user1", req, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*ParentAndChildCategoryMustMatch*");
        }
        [Fact]
        public async Task CreateAsync_with_missing_parent_throws_keynotfound()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var req = new CreateCategoryRequestDto
            {
                Name = "food",
                Type = CategoryType.Expense,
                ParentId = Guid.NewGuid()
            };

            var act = () => sut.CreateAsync("user-1", req, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*ParentCategoryNotFound*");
        }
        [Fact]
        public async Task CreateAsync_duplicate_name_same_type_for_user_throws_validation_exception()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            ctx.Categories.Add(TestData.Category("user1", "Groceries", CategoryType.Expense));
            await ctx.SaveChangesAsync();

            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var act = () => sut.CreateAsync("user1", new CreateCategoryRequestDto
            {
                Name = "Groceries",
                Type = CategoryType.Expense
            }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*CategoryWithTheSameNameAlreadyExists*");
        }
        [Fact]
        public async Task GetAllAsync_returns_only_users_nonarchived_ordered()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            ctx.Categories.AddRange(
                TestData.Category("user1", "B", CategoryType.Expense),
                TestData.Category("user1", "A", CategoryType.Income),
                TestData.Category("user1", "Z", CategoryType.Income, archived: true),
                TestData.Category("user2", "C", CategoryType.Expense)
            );
            await ctx.SaveChangesAsync();

            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var list = await sut.GetAllAsync("user1", CancellationToken.None);

            list.Should().HaveCount(2);
            list.Select(x => (x.CategoryType, x.Name)).Should().ContainInOrder(
                (CategoryType.Expense, "B"),
                (CategoryType.Income, "A")
            );
        }
        [Fact]
        public async Task CreateAsync_parent_belongs_to_other_user_throws_not_found()
        {
            using var ctx = TestRepositoryContext.CreateInMemory();
            var parentOfOtherUser = TestData.Category("other", "Parent", CategoryType.Expense);
            ctx.Categories.Add(parentOfOtherUser);
            await ctx.SaveChangesAsync();

            var sut = new CategoryService(ctx, new CreateCategoryRequestValidator(), TestData.NullLog<CategoryService>());
            var req = new CreateCategoryRequestDto
            {
                Name = "Child",
                Type = CategoryType.Expense,
                ParentId = parentOfOtherUser.Id
            };

            var act = () => sut.CreateAsync("u1", req, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                     .WithMessage("*ParentCategoryNotFound*");
        }
    }
}
