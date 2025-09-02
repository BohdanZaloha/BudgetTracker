using BudgetTracker.Application.Abstractions.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BudgetTracker.Infrastructure.Authentification
{
    internal class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _credentials;

        public JwtTokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }
        public string GenerateToken(string userId, string userName, string? email, IEnumerable<string> roles, IEnumerable<Claim>? extraClaims = null, DateTime? nowUtc = null)
        {
            var now = nowUtc ?? DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
            };
            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }
            if (extraClaims is not null)
            {
                claims.AddRange(extraClaims);
            }

            var token = new JwtSecurityToken
                (
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.AddMinutes(_options.AccessTokenMinutes),
                    signingCredentials: _credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
