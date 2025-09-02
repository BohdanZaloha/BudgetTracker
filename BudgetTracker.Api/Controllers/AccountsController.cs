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
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAll(CancellationToken token)
        {
            var response = await _accountService.GetAllAsync(User.GetUserId(), token);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequestDto createDto, CancellationToken token)
        {
            var created = await _accountService.CreateAsync(User.GetUserId(), createDto, token);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }
    }
}
