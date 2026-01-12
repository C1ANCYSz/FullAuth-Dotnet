using AuthApp.Config;
using AuthApp.Infrastructure.Database.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthApp.Infrastructure.Database;

public static class DbRegisteration
{
    public static WebApplicationBuilder RegisterDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<PasswordHashInterceptor>();

        builder.Services.AddDbContext<AppDbContext>(
            (sp, options) =>
            {
                var connectionString = sp.GetRequiredService<
                    IOptions<Env>
                >().Value.DB_CONNECTION_STRING;

                options.UseNpgsql(connectionString, o => o.EnableRetryOnFailure());

                options.AddInterceptors(sp.GetRequiredService<PasswordHashInterceptor>());
                //dotnet ef migrations add InitialCreate

                //dotnet ef database update
            }
        );

        return builder;
    }

    public static void ConnectDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            if (app.Environment.IsDevelopment())
            {
                db.Database.Migrate();
            }
            logger.LogInformation("PostgreSQL connected & migrations applied");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "PostgreSQL connection failed");
            throw;
        }
    }
}
