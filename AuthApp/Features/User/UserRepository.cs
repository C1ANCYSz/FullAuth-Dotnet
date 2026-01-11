using System;
using AuthApp.Features.Auth.DTOs;
using AuthApp.Features.User.DTOs;
using AuthApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Features.User;

public class UserRepository(AppDbContext db)
{

    public async Task<bool> FindUserById(Guid id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;
        return true;
    }

    public async Task<User?> FindUserByEmail(string email)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return null;
        return user;
    }

    public async Task<User?> GetUserProfile(Guid id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return null;
        return user;


    }


    public async Task<User> CreateUser(string email, string hashedPassword)
    {
        var user = new User
        {
            Email = email,
            Password = hashedPassword


        };
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUserProfile(Guid id, UpdateUserDto data)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return null;

        if (data.Bio is not null) user.Name = data.Bio;

        await db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAccount(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;

        db.Users.Remove(user);

        await db.SaveChangesAsync();

        return true;
    }



}
