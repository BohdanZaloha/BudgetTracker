using BudgetTracker.Application.Abstractions;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BudgetTracker.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepositoryContext _repository;
        private readonly IValidator<CreateAccountRequestDto> _validator;
        private readonly ILogger<AccountService> _logger;
        public AccountService(IRepositoryContext repository, IValidator<CreateAccountRequestDto> validator, ILogger<AccountService> logger)
        {
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }
        /// <summary>
        /// Creates a new account for <paramref name="userId"/>.
        /// </summary>
        public async Task<AccountDto> CreateAsync(string userId, CreateAccountRequestDto requestDto, CancellationToken token)
        {
            using var scope = _logger.BeginScope(new {AccountName = requestDto.Name });

            _logger.LogInformation("Creating account with currency {Currency}", requestDto.Currency);
            await _validator.ValidateAndThrowAsync(requestDto, token);
            var accountExists = await _repository.Accounts.AnyAsync(x => x.UserId == userId && x.Name == requestDto.Name, token);
            if (accountExists)
            {
                throw new ValidationException("AccountWithTheSameNameExists");
            }

            var account = new Account()
            {
                Guid = Guid.NewGuid(),
                UserId = userId,
                Name = requestDto.Name.Trim(),
                Currency = requestDto.Currency.ToUpperInvariant(),
                IsArchived = false

            };
            await _repository.Accounts.AddAsync(account, token);
            await _repository.SaveChangesAsync(token);

            _logger.LogInformation("Account {AccountId} created", account.Guid);
            return new AccountDto
            {
                Id = account.Guid,
                Name = account.Name,
                Currency = account.Currency,
                IsArchived = account.IsArchived
            };

        }

        /// <summary>
        /// Lists all non-archived accounts for <paramref name="userId"/>.
        /// </summary>
        public async Task<IReadOnlyList<AccountDto>> GetAllAsync(string userId, CancellationToken token)
        {
            _logger.LogInformation("Listing accounts");
            var accountList = await _repository.Accounts.Where(x => x.UserId == userId && !x.IsArchived).OrderBy(x => x.Name).Select(x => new AccountDto
            {
                Id = x.Guid,
                Name = x.Name,
                Currency = x.Currency,
                IsArchived = x.IsArchived
            }).ToListAsync(token);

            _logger.LogInformation("Found {Count} accounts", accountList.Count);
            return accountList;
        }
    }
}
