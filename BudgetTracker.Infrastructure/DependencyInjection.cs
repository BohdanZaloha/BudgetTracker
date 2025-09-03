using BudgetTracker.Application.Abstractions;
using BudgetTracker.Application.Abstractions.Security;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Models;
using BudgetTracker.Infrastructure.Authentification;
using BudgetTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BudgetTracker.Infrastructure
{
    /// <summary>
    /// Adds database context, Identity, JWT authentication, and infrastructure services.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<RepositoryContext>(options => options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(RepositoryContext).Assembly.FullName)));
            services.AddScoped<IRepositoryContext>(sp => sp.GetRequiredService<RepositoryContext>());
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
               .AddEntityFrameworkStores<RepositoryContext>()
               .AddDefaultTokenProviders();

            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt settings are required");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
.AddJwtBearer(b =>
{
    b.RequireHttpsMetadata = false;
    b.SaveToken = true;

    b.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,

        ValidateAudience = true,
        ValidAudience = jwt.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    b.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var hasAuth = context.Request.Headers.ContainsKey("Authorization");
            if (!hasAuth) Console.WriteLine("No Authorization header on request to " + context.Request.Path);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var sub = context.Principal?.FindFirst("sub")?.Value;
            Console.WriteLine("JWT validated for sub: " + sub);
            return Task.CompletedTask;
        }
    };
});

            services.AddAuthorization();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.Configure<AuthService.JwtOptionsSnapshot>(o =>
            {
                o.AccessTokenMinutes = jwt.AccessTokenMinutes;
            });
            return services;
        }
    }
}
