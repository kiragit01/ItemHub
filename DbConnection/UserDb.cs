using ItemHub.Data;
using ItemHub.DbConnection.Interfaces;
using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ItemHub.DbConnection;

public class UserDb(DataBaseContext db) : IUserDb
{
    public Task<User?> GetUser(string login) => db.Users
        .Include(user => user.Items)
        .FirstOrDefaultAsync(u => u.Login == login);

    public async Task<DebugMessage> CheckUser(string? email, string? login)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            return DebugMessage.ErrorEmail;
        }
        user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user != null)
        {
            return DebugMessage.ErrorLogin;
        }
        return DebugMessage.Success;
    }
    
    public async Task<User?> SingIn(string? login, string? password) 
        => await db.Users
            .FirstOrDefaultAsync(u => (u.Email == login || u.Login == login) && u.HashedPassword == password);

    public async Task AddUser(User user)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }


    public async Task DeleteUser(User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
}