using AuthApp.Common.Utils;
using AuthApp.Features.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthApp.Interceptors;

public sealed class PasswordHashInterceptor : SaveChangesInterceptor
{
    private static void HashPasswords(DbContext context)
    {
        var entries = context.ChangeTracker
            .Entries<User>()
            .Where(e =>
                e.State == EntityState.Added ||
                (e.State == EntityState.Modified &&
                 e.Property(nameof(User.Password)).IsModified)
            );

        foreach (var entry in entries)
        {
            var password = entry.Entity.Password;

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Password cannot be empty");

            if (PasswordUtil.IsHashed(password))
                continue;

            entry.Entity.Password = PasswordUtil.Hash(password);
        }
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            HashPasswords(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            HashPasswords(eventData.Context);

        return ValueTask.FromResult(result);
    }
}
