
using BudgetTracker.Api.MiddleWare;
using BudgetTracker.Application;
using BudgetTracker.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Context;

namespace BudgetTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.Console().CreateBootstrapLogger();
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((ctx, services, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(services).Enrich.FromLogContext()
);
            // Add services to the container.
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer(); //?
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BudgetTracker API", Version = "v1" });

                // 1) Define the Bearer scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the JWT (no Bearer prefix, no quotes)."
                });

                // 2) Require it globally — reference by id!
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.Use(async (ctx, next) =>
            {
                var userId = ctx.User?.FindFirst("sub")?.Value
                          ?? ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
                using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
                using (LogContext.PushProperty("RequestPath", (string)ctx.Request.Path))
                {
                    await next();
                }
            });

            app.UseSwagger();
            app.UseSwaggerUI();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseSerilogRequestLogging(o =>
            {
                o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                o.EnrichDiagnosticContext = (diag, http) =>
                {
                    diag.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString());
                    diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
                };
            });

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
}
