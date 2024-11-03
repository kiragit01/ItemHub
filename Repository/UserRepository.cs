using ItemHub.Data;
using ItemHub.Interfaces;
using ItemHub.Models.User;
using ItemHub.Repository.Interfaces;
using ItemHub.Services;
using ItemHub.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Repository;

public class UserRepository(DataBaseContext db, IUserContext userContext) : IUserRepository
{
    public Task<User?> GetUserAsync(string? login = null)
    {
        var contextLogin = login ?? userContext.Login;
        return db.Users
            .Include(user => user.CustomItems)
            .FirstOrDefaultAsync(u => u.Login == contextLogin);
    }

    public async Task<ResponseMessage> CheckUserAsync(string? email, string? login)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            return ResponseMessage.ErrorEmail;
        }
        user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
        return user != null 
            ? ResponseMessage.ErrorLogin 
            : ResponseMessage.Ok;
    }

    public async Task<User?> LogInAsync(string login, string password)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user == null) return null;
        var checkPassword = HashedPassword.Hashed(password, user.Salt);
        return user.HashedPassword == checkPassword 
            ? user 
            : null;
    }

    public async Task AddUserAsync(User user)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }


    public async Task DeleteUserAsync(User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
}