using AuthApp.Config;
using AuthApp.Infrastructure.Database;
using AuthApp.Middlewares;
using AuthApp.Features;
var builder = WebApplication.CreateBuilder(args);


builder
.RegisterEnvVariables()
.RegisterDbContext()
.RegisterFeatures();


builder.Services.AddControllers();

var app = builder.Build();

app.ConnectDb();

app.UseMiddleware<GlobalExceptionMiddleware>();


app.Run();

