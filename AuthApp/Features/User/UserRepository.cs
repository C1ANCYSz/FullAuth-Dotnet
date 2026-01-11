using System;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.User.DTOs;
using AuthApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Features.User;

public class UserRepository(AppDbContext db)
{

    public async Task<Guid?> FindUserById(string id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Guid.Parse(id));
        if (user is null) return null;
        return user.Id;
    }

    public async Task<Guid?> FindUserByEmail(string email)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return null;
        return user.Id;
    }

    public async Task<UserDto?> GetUserProfile(string id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Guid.Parse(id));
        if (user is null) return null;
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Dob = user.Dob
        };


    }


    public async Task<UserDto?> CreateUser(SignupDto data)
    {
        var user = new User
        {
            Email = data.Email,
            Password = data.Password,


        };
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<UserDto?> UpdateUserProfile(Guid id, UpdateUserDto data)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return null;

        if (data.Bio is not null) user.Name = data.Bio;

        await db.SaveChangesAsync();
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Dob = user.Dob,
            Bio = user.Bio
        };
    }

    public async Task<bool> DeleteAccount(Guid id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;

        db.Users.Remove(user);

        await db.SaveChangesAsync();

        return true;
    }



}
