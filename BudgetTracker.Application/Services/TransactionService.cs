using BudgetTracker.Application.Abstractions;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepositoryContext _repository;
        private readonly IValidator<CreateTransactionRequestDto> _CreateValidator;
        private readonly IValidator<TransactionQuery> _queryValidator;
        private readonly ILogger<TransactionService> _logger;
        public TransactionService(IRepositoryContext repository, IValidator<CreateTransactionRequestDto> CreateValidator, IValidator<TransactionQuery> queryValidator, ILogger<TransactionService> logger)
        {
            _CreateValidator = CreateValidator;
            _queryValidator = queryValidator;
            _repository = repository;
            _logger = logger;
        }
        public async Task<TransactionDto> CreateAsync(string userId, CreateTransactionRequestDto requestDto, CancellationToken token)
        {
            using var scope = _logger.BeginScope(new {AccountId = requestDto.AccountId });

            _logger.LogInformation("Creating transaction {Type} {Amount} {Currency}", requestDto.Type, requestDto.Amount, requestDto.Currency);

            await _CreateValidator.ValidateAndThrowAsync(requestDto, token);

            var accountExists = await _repository.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsArchived && x.Guid == requestDto.AccountId, token);
            if (accountExists == null)
            {
                throw new KeyNotFoundException("account not FOUND");
            }
            var currency = requestDto.Currency.ToUpperInvariant();
            if (currency != accountExists.Currency)
            {
                throw new ValidationException("Currency does not match the account currency"); //????
            }

            Category? category = null;
            if (requestDto.CategoryId.HasValue)
            {
                category = await _repository.Categories.FirstOrDefaultAsync(c => c.Id == requestDto.CategoryId && c.UserId == userId && !c.IsArchived, token);
                if (category == null)
                {
                    throw new KeyNotFoundException("Category Was not found");
                }
                if ((category.Type == CategoryType.Expense && requestDto.Type != TransactionType.Expense) || (category.Type == CategoryType.Income && requestDto.Type != TransactionType.Income))
                {
                    throw new ValidationException("Category type must match transaction type");
                }
            }

            var happenedAtUtc = (requestDto.OccurredAtUtc?.ToUniversalTime()) ?? DateTime.UtcNow;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = requestDto.AccountId,
                Type = requestDto.Type,
                Amount = requestDto.Amount,
                Currency = currency,
                CategoryId = category?.Id,
                OccurredAtUtc = happenedAtUtc,
                Note = string.IsNullOrWhiteSpace(requestDto.Note) ? null : requestDto.Note.Trim(),
                IsDeleted = false
            };
            await _repository.Transactions.AddAsync(transaction, token);
            await _repository.SaveChangesAsync(token);

            _logger.LogInformation("Transaction {TransactionId} created on account {AccountId}", transaction.Id, transaction.AccountId);


            return new TransactionDto
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                TransactionType = transaction.Type,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                CategoryId = transaction.CategoryId,
                OccurredAtUtc = transaction.OccurredAtUtc,
                Note = transaction.Note
            };
        }



        public async Task<PagedResult<TransactionDto>> QueryAsync(string userId, TransactionQuery query, CancellationToken token)
        {

            _logger.LogInformation("Query transactions page {Page} size {PageSize} from {From} to {To} type {Type} account {Account} category {Category}",
                query.Page, query.PageSize, query.FromUtc, query.ToUtc, query.Type, query.AccountId, query.CategoryId);

            await _queryValidator.ValidateAndThrowAsync(query, token);

            var q = _repository.Transactions.Where(x => x.UserId == userId && !x.IsDeleted);

            if (query.FromUtc.HasValue) q = q.Where(t => t.OccurredAtUtc >= query.FromUtc.Value);
            if (query.ToUtc.HasValue) q = q.Where(t => t.OccurredAtUtc <= query.ToUtc.Value);
            if (query.Type.HasValue) q = q.Where(t => t.Type == query.Type.Value);
            if (query.CategoryId.HasValue) q = q.Where(t => t.CategoryId == query.CategoryId.Value);
            if (query.AccountId.HasValue) q = q.Where(t => t.AccountId == query.AccountId.Value);

            var total = await q.CountAsync(token);

            var items = await q
                .OrderByDescending(t => t.OccurredAtUtc)
                .ThenByDescending(t => t.CreatedAtUtc)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    AccountId = t.AccountId,
                    TransactionType = t.Type,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    CategoryId = t.CategoryId,
                    OccurredAtUtc = t.OccurredAtUtc,
                    Note = t.Note
                })
                .ToListAsync(token);
            _logger.LogInformation("Query returned {Total} items (page {Page})", items.Count(), query.Page);
            return new PagedResult<TransactionDto>
            {
                Items = items,
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
