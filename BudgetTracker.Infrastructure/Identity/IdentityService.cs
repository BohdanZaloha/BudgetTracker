using BudgetTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace BudgetTracker.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        public IdentityService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
        {
            _signIn = signIn;
            _users = users;
        }
        public async Task<(bool Succeeded, string? UserId, string[] Errors)> CreateUserAsync(string email, string? userName, string password, CancellationToken token)
        {
            var user = new ApplicationUser
            {
                Email = email,
                UserName = string.IsNullOrWhiteSpace(userName) ? email : userName
            };
            var response = await _users.CreateAsync(user, password);
            return response.Succeeded
                    ? (true, user.Id, Array.Empty<string>())
                    : (false, null, response.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<(string UserId, string UserName, string? Email)?> FindByEmailAsync(string email, CancellationToken token)
        {
            var user = await _users.FindByEmailAsync(email) ?? throw new InvalidOperationException("User not found after successful sign-in.");
            return user is null ? null : (user.Id, user.UserName!, user.Email);
        }

        public async Task<string[]> GetRolesAsync(string userId, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(userId);
            if (user == null)
            {
                return Array.Empty<string>();
            }
            var roles = await _users.GetRolesAsync(user);
            return roles.ToArray();
        }

        public async Task<(bool Succeeded, string UserId)> ValidateCredentialsAsync(string email, string password, CancellationToken token)
        {
            var user = await _users.FindByEmailAsync(email);
            if (user == null)
            {
                return (false, string.Empty);
            }

            var pass = await _signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            return pass.Succeeded ? (true, user.Id) : (false, string.Empty);
        }
    }
}
