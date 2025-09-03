using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BudgetTracker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthenticationController(IAuthService auth)
        {
            _auth = auth;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthentificationResult>> Register([FromBody] RegisterUserCommand cmd, CancellationToken token)
        {
            var result = await _auth.RegisterAsync(cmd, token);
            return Ok(result);
        }

        /// <summary>
        /// Authenticates a user.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthentificationResult>> Login([FromBody] LoginUserCommand cmd, CancellationToken token)
        {
            var result = await _auth.LoginAsync(cmd, token);
            return Ok(result);
        }

        /// <summary>
        /// Returns basic information about the current authenticated user.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                name = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name,
                email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtRegisteredClaimNames.Email),
                roles = User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type.EndsWith("/role")).Select(c => c.Value)
            });
        }
    }
}
