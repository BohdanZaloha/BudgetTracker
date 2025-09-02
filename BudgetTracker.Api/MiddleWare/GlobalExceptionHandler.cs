using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Api.MiddleWare
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
            _environment = environment;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation($"Request Cancelled: {httpContext.Request.Method} {httpContext.Request.Path}");
                return true;
            }

            var (status, title, code) = Map(exception);
            
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ShouldShowDetail(exception, status) ? exception.Message : null,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.com/{status}"

            };
            problem.Extensions["traceId"] = traceId;
            problem.Extensions["code"] = code;
            if (exception is ValidationException ve)
            {
                var errors = ve.Errors
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "" : e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
                problem.Extensions["errors"] = errors;
            }
            if (status >= 500)
                _logger.LogError(exception, "{Title} {Path} → {Status} ({TraceId})", title, httpContext.Request.Path, status, traceId);
            else
                _logger.LogWarning("{Title} {Path} → {Status} ({TraceId}) | {Message}", title, httpContext.Request.Path, status, traceId, exception.Message);

            httpContext.Response.StatusCode = status;
            // Leverage content negotiation (ProblemDetailsService handles JSON/XML/etc.)
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
        }

        private static (int status, string title, string code) Map(Exception exception)
        {
            if (exception is UnauthorizedAccessException)
            {
                return (401, "Unauthorized", "auth.unauthorized");
            }
            if (exception is KeyNotFoundException)
            {
                return (404, "Not Found", "common.not_found");
            }
            if (exception is BadHttpRequestException) 
                    {
                return (400, "Bad Request", "http.bad_request");
            }
            if (exception is System.Text.Json.JsonException)
            {
                return (400, "Invalid JSON", "http.invalid_json");
            }
            if (exception is ValidationException)
            {
                return (400, "Validation Failed", "validation.failed");
            }
            if (exception is DbUpdateException)
            {
                return (409, "Conflict", "db.conflict");
            }
            if(exception is OperationCanceledException)
            {
                return (499, "Client Closed Request", "request.canceled");
            }
            return (500, "Internal Server Error", "common.unhandled");
        }
        private bool ShouldShowDetail(Exception ex, int status)
       => _environment.IsDevelopment() || status is >= 400 and < 500;
    }
}
