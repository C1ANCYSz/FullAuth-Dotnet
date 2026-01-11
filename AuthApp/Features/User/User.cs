
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthApp.Features.User;

public class User
{
    public Guid Id { get; set; }

    //required
    public required string Email { get; set; }
    public required string Password { get; set; }

    //Onboard Required
    public string Name { get; set; } = string.Empty;
    public DateOnly Dob { get; set; }

    //Optional
    public string Bio { get; set; } = string.Empty;

    //Meta
    public int TokenVersion { get; set; } = 0;
    public bool IsOnboard { get; set; } = false;


}




public class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();



        entity.Property(u => u.Email).IsRequired();
        entity.Property(u => u.TokenVersion).HasDefaultValue(0);
        entity.HasIndex(u => u.Email).IsUnique();
        entity.Property(u => u.IsOnboard).HasDefaultValue(false);

        entity.Property(u => u.Password).IsRequired();


    }
}