
using BudgetTracker.Application.Abstractions.Security;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BudgetTracker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityService _identity;
        private readonly IJwtTokenGenerator _jwt;
        private readonly int _accessMinutes;
        private readonly ILogger<AuthService> _logger;
        public sealed class JwtOptionsSnapshot
        {
            public int AccessTokenMinutes { get; set; } = 60;
        }
        public AuthService(IIdentityService identity, IJwtTokenGenerator jwt, IOptions<JwtOptionsSnapshot> jwtOptions, ILogger<AuthService> logger)
        {
            _identity = identity;
            _jwt = jwt;
            _accessMinutes = jwtOptions.Value.AccessTokenMinutes;
            _logger = logger;
        }
        public async Task<AuthentificationResult> LoginAsync(LoginUserCommand cmd, CancellationToken token)
        {
            _logger.LogInformation("Login attempt for {Email}", cmd.Email);
            var (success, userId) = await _identity.ValidateCredentialsAsync(cmd.Email, cmd.Password, token);
            if (!success)
            {
                throw new UnauthorizedAccessException("Invalid Email or password");
            }
            var roles = await _identity.GetRolesAsync(userId, token);
            var me = await _identity.FindByEmailAsync(cmd.Email, token);
            var jwtToken = _jwt.GenerateToken(userId, me.Value.UserName ?? cmd.Email, me.Value.Email, roles);
            _logger.LogInformation("Login succeeded for {UserId}; token expires in {ExpiresMinutes})", userId, _accessMinutes);
            return new AuthentificationResult
            {
                AccessToken = jwtToken,
                ExpiresInSeconds = _accessMinutes * 60,
                UserId = userId,
                Email = me.Value.Email!,
                UserName = me.Value.UserName!,
                Roles = roles
            };
        }

        public async Task<AuthentificationResult> RegisterAsync(RegisterUserCommand cmd, CancellationToken token)
        {
            _logger.LogInformation("Register attempt for {Email}", cmd.Email);
            var (success, userId, errors) = await _identity.CreateUserAsync(cmd.Email, cmd.UserName, cmd.Password, token);
            if (!success)
            {
                throw new InvalidOperationException(string.Join("; ", errors));
            }

            var roles = await _identity.GetRolesAsync(userId!, token);
            var me = await _identity.FindByEmailAsync(cmd.Email, token);
            var jwtToken = _jwt.GenerateToken(userId!, me?.UserName ?? cmd.Email, me?.Email, roles);

            _logger.LogInformation("Register succeeded for {UserId}; token expires in {ExpiresMinutes})", userId, _accessMinutes);
            return new AuthentificationResult
            {
                AccessToken = jwtToken,
                ExpiresInSeconds = _accessMinutes * 60,
                UserId = userId!,
                Email = me?.Email ?? cmd.Email,
                UserName = me?.UserName ?? (cmd.UserName ?? cmd.Email),
                Roles = roles
            };
        }
    }
}
