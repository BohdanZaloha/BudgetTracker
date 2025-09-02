using BudgetTracker.Application.Abstractions;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepositoryContext _repository;
        private readonly IValidator<CreateCategoryRequestDto> _validator;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(IRepositoryContext repository, IValidator<CreateCategoryRequestDto> validator, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }
        public async Task<CategoryDto> CreateAsync(string userId, CreateCategoryRequestDto requestDto, CancellationToken token)
        {

            _logger.LogInformation("Creating Category with name {Name}", requestDto.Name);
            await _validator.ValidateAndThrowAsync(requestDto, token);

            Category? parent = null;
            if (requestDto.ParentId.HasValue)
            {
                parent = await _repository.Categories.FirstOrDefaultAsync(x => x.Id == requestDto.ParentId && x.UserId == userId, token);
                if (parent == null) throw new KeyNotFoundException("ParentCategoryNotFound");
                if (parent.Type != requestDto.Type) throw new ValidationException("ParentAndChildCategoryMustMatch");

            }
            var categoryExists = await _repository.Categories.AnyAsync(x => x.UserId == userId && x.Type == requestDto.Type && x.Name == requestDto.Name, token);
            if (categoryExists)
            {
                throw new ValidationException("CategoryWithTheSameNameAlreadyExists");
            }
            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = requestDto.Name,
                Type = requestDto.Type,
                ParentId = parent?.Id
            };
            _repository.Categories.Add(category);
            await _repository.SaveChangesAsync(token);
            _logger.LogInformation("Category {CategoryId} created", category.Id);
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                CategoryType = category.Type,
                ParentId = category.ParentId,
                IsArchived = category.IsArchived
            };
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(string userId, CancellationToken token)
        {
            _logger.LogInformation("Listing categories");

            var categoryList = await _repository.Categories.Where(x => x.UserId == userId && !x.IsArchived).OrderBy(x => x.Type).ThenBy(x => x.Name).Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryType = x.Type,
                ParentId = x.ParentId,
                IsArchived = x.IsArchived
            }).ToListAsync(token);

            _logger.LogInformation("Found {Count} categories", categoryList.Count);

            return categoryList;
        }
    }
}
