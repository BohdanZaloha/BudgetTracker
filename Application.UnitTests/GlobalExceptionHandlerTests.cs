using Application.UnitTests.Testing;
using BudgetTracker.Api.MiddleWare;
using BudgetTracker.Application.Abstractions.Security;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.DTOS;
using BudgetTracker.Application.Services;
using BudgetTracker.Application.Validation;
using BudgetTracker.Infrastructure.Authentification;
using BudgetTracker.Infrastructure.Identity;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.UnitTests
{
    public class GlobalExceptionHandlerTests
    {
        [Fact]
        public async Task TryHandleAsync_for_validation_exception_writes_problem_with_errors()
        {
            var problemSvc = new Mock<IProblemDetailsService>();
            ProblemDetails? captured = null;
            problemSvc.Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
                      .Callback<ProblemDetailsContext>(ctx => captured = ctx.ProblemDetails)
                      .ReturnsAsync(true);

            var env = new Mock<IHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");

            var handler = new GlobalExceptionHandler(problemSvc.Object, Mock.Of<ILogger<GlobalExceptionHandler>>(), env.Object);

            var http = new DefaultHttpContext();
            http.Request.Path = "/api/test";

            var ve = new ValidationException(new[]
            {
            new FluentValidation.Results.ValidationFailure("Amount","must be > 0"),
            new FluentValidation.Results.ValidationFailure("","generic")
        });

            var handled = await handler.TryHandleAsync(http, ve, CancellationToken.None);

            handled.Should().BeTrue();
            captured.Should().NotBeNull();
            captured!.Status.Should().Be(400);
            captured.Title.Should().Be("Validation Failed");
            captured.Extensions.Should().ContainKey("errors");
            captured.Extensions.Should().ContainKey("code");
            captured.Extensions.Should().ContainKey("traceId");
        }

        [Fact]
        public async Task TryHandleAsync_when_request_aborted_returns_true_without_writing()
        {
            var problemSvc = new Mock<IProblemDetailsService>(MockBehavior.Strict);
            var env = new Mock<IHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");

            var handler = new GlobalExceptionHandler(problemSvc.Object, Mock.Of<ILogger<GlobalExceptionHandler>>(), env.Object);

            var http = new DefaultHttpContext();
            var cts = new CancellationTokenSource();
            http.RequestAborted = cts.Token;
            cts.Cancel();

            var handled = await handler.TryHandleAsync(http, new OperationCanceledException(), CancellationToken.None);

            handled.Should().BeTrue();
        }
    }
}
