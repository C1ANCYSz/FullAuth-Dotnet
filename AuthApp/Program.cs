using AuthApp.Common.Middleware;
using AuthApp.Common.RateLimit;
using AuthApp.Config;
using AuthApp.Features;
using AuthApp.Infrastructure;
using AuthApp.Infrastructure.Auth;
using AuthApp.Infrastructure.Database;
using AuthApp.Infrastructure.Redis;

var builder = WebApplication.CreateBuilder(args);

builder
    .RegisterEnvVariables()
    .RegisterJwtOptions()
    .AddAuthWithJwt()
    .RegisterDbContext()
    .AddFluentValidation()
    .RegisterFeatures()
    .RegisterRedisClient()
    .RegisterRateLimits();

builder.Services.AddControllers();

var app = builder.Build();

app.ConnectDb();
app.UseRateLimiter();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();
