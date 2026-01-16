using AuthApp.Common.Middleware;
using AuthApp.Common.RateLimit;
using AuthApp.Config;
using AuthApp.Features;
using AuthApp.Infrastructure;
using AuthApp.Infrastructure.Auth;
using AuthApp.Infrastructure.Auth.Providers;
using AuthApp.Infrastructure.Database;
using AuthApp.Infrastructure.Email;
using AuthApp.Infrastructure.Redis;

var builder = WebApplication.CreateBuilder(args);

builder
    .RegisterEnvVariables()
    .RegisterJwtOptions()
    .RegisterOAuthOptions()
    .AddAuthWithJwt()
    .RegisterDbContext()
    .AddFluentValidation()
    .RegisterFeatures()
    .RegisterRedisClient()
    .RegisterSmtpClient()
    .RegisterOAuthProviders();

// .RegisterRateLimits();

builder.Services.AddControllers();

var app = builder.Build();

app.ConnectDb();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RedisRateLimitMiddleware>();
app.MapControllers();

app.Run();
