

using AuthApp.Config;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AuthApp.Features.User;

namespace AuthApp.Infrastructure.Database;

public static class DbRegisteration
{
    public static WebApplicationBuilder RegisterDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IOptions<Env>>().Value.DB_CONNECTION_STRING;

            options.UseNpgsql(connectionString,
             o => o.EnableRetryOnFailure());

        });

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
