using AuthApp.Config;
using AuthApp.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder
.RegisterEnvVariables()
.RegisterDbContext();

var app = builder.Build();

app.ConnectDb();





app.Run();

