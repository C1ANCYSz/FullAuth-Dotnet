

using AuthApp.Features.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AuthApp.Infrastructure.Security;

namespace AuthApp.Interceptors;

public sealed class PasswordHashInterceptor : SaveChangesInterceptor
{
    private static void HashPasswords(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<User>()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (!entry.Property(u => u.Password).IsModified &&
                entry.State != EntityState.Added)
                continue;

            var password = entry.Entity.Password;

            if (password.StartsWith("$2"))
                continue;

            entry.Entity.Password = PasswordHasher.Hash(password);
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context != null)
            HashPasswords(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            HashPasswords(eventData.Context);

        return ValueTask.FromResult(result);
    }
}
