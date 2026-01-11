using AuthApp.Config;
using AuthApp.Infrastructure.Database;
using AuthApp.Middlewares;
using AuthApp.Features;
using AuthApp.Infrastructure.Auth;
using AuthApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder
.RegisterEnvVariables()
.RegisterJwtOptions()
.AddAuthWithJwt()
.RegisterDbContext()
.AddFluentValidation()
.RegisterFeatures();


builder.Services.AddControllers();

var app = builder.Build();

app.ConnectDb();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();

