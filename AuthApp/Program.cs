using AuthApp.Config;
using AuthApp.Infrastructure.Database;
using AuthApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder
.RegisterEnvVariables()
.RegisterDbContext();

builder.Services.AddControllers();

var app = builder.Build();

app.ConnectDb();

app.UseMiddleware<GlobalExceptionMiddleware>();


app.Run();

