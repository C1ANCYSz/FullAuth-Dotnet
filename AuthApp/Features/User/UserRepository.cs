using AuthApp.Features.User.DTOs;
using AuthApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Features.User;

public class UserRepository(AppDbContext db)
{
    private AppDbContext _db = db;

    public async Task<User?> FindUserById(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return null;
        return user;
    }

    public async Task<User?> FindUserByIdAsNoTracking(Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return null;
        return user;
    }

    public async Task<User?> GetUserForTokenRotation(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> FindUserByEmail(string email)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return null;
        return user;
    }

    public async Task<User?> GetUserProfile(Guid id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return null;
        return user;
    }

    public async Task<User> CreateUser(string email, string hashedPassword)
    {
        var user = new User { Email = email, Password = hashedPassword };
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUserProfile(Guid id, UpdateProfileDto data)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return null;

        if (data.Bio is not null)
            user.Bio = data.Bio;

        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> Onboard(Guid id, OnboardDto data)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return null;

        user.Dob = data.Dob;
        user.Name = data.Name;
        user.IsOnboard = true;
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteAccount(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
            return false;

        _db.Users.Remove(user);

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}
