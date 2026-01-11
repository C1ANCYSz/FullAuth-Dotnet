
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApp.Features.User;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateOnly Dob { get; set; }
    public required int TokenVersion { get; set; }
}




public class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Name).IsRequired();

        entity.Property(u => u.Email).IsRequired();
        entity.HasIndex(u => u.Email).IsUnique();

        entity.Property(u => u.Password).IsRequired();

        entity.Property(u => u.Dob).IsRequired();
    }
}