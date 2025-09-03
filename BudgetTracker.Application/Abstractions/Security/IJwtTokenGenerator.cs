using System.Security.Claims;

namespace BudgetTracker.Application.Abstractions.Security
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Creates a signed JWT access token for a user.
        /// </summary>
        string GenerateToken(string userId, string userName, string? email, IEnumerable<string> roles, IEnumerable<Claim>? extraClaims = null, DateTime? nowUtc = null);
    }
}
