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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Gets all categories for the current user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken token)
        {
            var response = await _categoryService.GetAllAsync(User.GetUserId(), token);
            return Ok(response);
        }

        /// <summary>
        /// Creates a new category for the current user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequestDto createDto, CancellationToken token)
        {
            var created = await _categoryService.CreateAsync(User.GetUserId(), createDto, token);
            return Created($"api/categories/{created.Id}", created);
        }
    }
}
