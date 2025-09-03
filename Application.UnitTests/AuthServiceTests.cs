using Application.UnitTests.Testing;
using BudgetTracker.Application.Abstractions.Security;
using BudgetTracker.Application.Dtos;
using BudgetTracker.Application.Services;
using BudgetTracker.Infrastructure.Identity;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests
{
    public class AuthServiceTests
    {
        private static AuthService Svc(IIdentityService identity, IJwtTokenGenerator jwt, int minutes = 15)
        {
            return new AuthService(identity, jwt, Options.Create(new AuthService.JwtOptionsSnapshot { AccessTokenMinutes = minutes }), TestData.NullLog<AuthService>());
        }
        [Fact]
        public async Task LoginAsync_success_returns_token_and_user_info()
        {
            var identity = new Mock<IIdentityService>();
            var jwt = new Mock<IJwtTokenGenerator>();

            identity.Setup(i => i.ValidateCredentialsAsync("a@b.com", "P@ss", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((true, "user1"));
            identity.Setup(i => i.GetRolesAsync("user1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new[] { "User", "Admin" });
            identity.Setup(i => i.FindByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(("user1", "Alice", "a@b.com"));
            jwt.Setup(j => j.GenerateToken("user1", "Alice", "a@b.com", It.IsAny<IEnumerable<string>>(), null, null))
               .Returns("token-123");

            var result = await Svc(identity.Object, jwt.Object, minutes: 30).LoginAsync(new LoginUserCommand { Email = "a@b.com", Password = "P@ss" }, CancellationToken.None);

            result.AccessToken.Should().Be("token-123");
            result.ExpiresInSeconds.Should().Be(30 * 60);
            result.UserId.Should().Be("user1");
            result.UserName.Should().Be("Alice");
            result.Email.Should().Be("a@b.com");
            result.Roles.Should().BeEquivalentTo(new[] { "User", "Admin" });
        }

        [Fact]
        public async Task LoginAsync_when_invalid_credentials_throws_unauthorized()
        {
            var identity = new Mock<IIdentityService>();
            identity.Setup(i => i.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((false, string.Empty));

            var jwt = new Mock<IJwtTokenGenerator>();

            var act = () => Svc(identity.Object, jwt.Object).LoginAsync(new LoginUserCommand { Email = "a@b.com", Password = "bad" }, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Invalid Email or password*");
        }

        [Fact]
        public async Task RegisterAsync_success_returns_token_and_user_info()
        {
            var identity = new Mock<IIdentityService>();
            var jwt = new Mock<IJwtTokenGenerator>();

            identity.Setup(i => i.CreateUserAsync("a@b.com", "Alice", "P@ss", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((true, "user1", Array.Empty<string>()));
            identity.Setup(i => i.GetRolesAsync("user1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Array.Empty<string>());
            identity.Setup(i => i.FindByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(("user1", "Alice", "a@b.com"));

            jwt.Setup(j => j.GenerateToken("user1", "Alice", "a@b.com", It.IsAny<IEnumerable<string>>(), null, null))
               .Returns("token-xyz");

            var result = await Svc(identity.Object, jwt.Object, minutes: 10).RegisterAsync(new RegisterUserCommand { Email = "a@b.com", Password = "P@ss", UserName = "Alice" }, CancellationToken.None);

            result.AccessToken.Should().Be("token-xyz");
            result.ExpiresInSeconds.Should().Be(600);
            result.UserId.Should().Be("user1");
            result.Email.Should().Be("a@b.com");
            result.UserName.Should().Be("Alice");
            result.Roles.Should().BeEmpty();
        }

        [Fact]
        public async Task RegisterAsync_failure_throws_invalidoperation_with_errors()
        {
            var identity = new Mock<IIdentityService>();
            var jwt = new Mock<IJwtTokenGenerator>();

            identity.Setup(i => i.CreateUserAsync("a@b.com", null, "bad", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((false, (string?)null, new[] { "Weak password", "Email in use" }));

            var act = () => Svc(identity.Object, jwt.Object).RegisterAsync(new RegisterUserCommand { Email = "a@b.com", Password = "bad" }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Weak password*Email in use*");
        }
    }
}
