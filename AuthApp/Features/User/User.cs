using System.ComponentModel.DataAnnotations;
using AuthApp.Common.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApp.Features.User;

public class User
{
    public Guid Id { get; set; }

    // Core identity
    public required string Email { get; set; }

    // Credentials auth only
    public string? Password { get; set; }

    // Onboarding
    public string Name { get; set; } = string.Empty;
    public DateOnly? Dob { get; set; }
    public string Bio { get; set; } = string.Empty;

    // Auth provider
    public AuthProvider Provider { get; set; } = AuthProvider.CREDENTIALS;
    public string? ProviderId { get; set; }

    // Meta
    public int TokenVersion { get; set; } = 0;
    public bool IsOnboard { get; set; } = false;

    public bool IsVerified { get; set; } = false;

    // Concurrency
    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
}

public sealed class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()").ValueGeneratedOnAdd();

        entity.Property(u => u.Email).IsRequired();

        entity.HasIndex(u => u.Email).IsUnique();

        entity.Property(u => u.Password).IsRequired(false);

        entity
            .Property(u => u.Provider)
            .HasConversion<int>()
            .HasDefaultValue(AuthProvider.CREDENTIALS)
            .IsRequired();

        entity.Property(u => u.ProviderId).IsRequired(false);

        entity
            .HasIndex(u => new { u.Provider, u.ProviderId })
            .IsUnique()
            .HasFilter("\"ProviderId\" IS NOT NULL");

        entity.Property(u => u.TokenVersion).HasDefaultValue(0);

        entity.Property(u => u.IsVerified).HasDefaultValue(false);

        entity.Property(u => u.IsOnboard).HasDefaultValue(false);
    }
}
