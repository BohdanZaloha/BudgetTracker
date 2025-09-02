using BudgetTracker.Application.Common;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Application.Services;
using BudgetTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [HttpGet]

        public async Task<ActionResult<PagedResult<TransactionDto>>> Query([FromQuery] TransactionQuery query, CancellationToken token)
        {
            var response = await _transactionService.QueryAsync(User.GetUserId(), query, token);
            return Ok(response);
        }
        //public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetAll(CancellationToken token)
        //{
        //    var response = await _transactionService.GetAllAsync(User.GetUserId(), token);
        //    return Ok(response);
        //}

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionRequestDto createDto, CancellationToken token)
        {
            var created = await _transactionService.CreateAsync(User.GetUserId(), createDto, token);
            return Created($"api/transactions/{created.Id}", created);
        }
    }
}
