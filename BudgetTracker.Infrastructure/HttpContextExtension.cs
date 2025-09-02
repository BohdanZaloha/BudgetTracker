using System.Security.Claims;

namespace BudgetTracker.Infrastructure
{
    public static class HttpContextExtension
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        }
    }
}
