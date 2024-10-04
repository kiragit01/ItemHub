using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;

namespace ItemHub.DbConnection.Interfaces;

public interface IUserDb
{
    public Task AddUser(User user);
    public Task<User?> GetUser(string login);
    public Task<DebugMessage> CheckUser(string email, string login);
    public Task<User?> SingIn(string login, string password);
    public Task UpdateUser(User user);
    public Task DeleteUser(User user);
    
    public Task<Item?> UserItems(Guid id);
    public Task UserItems(Item item);
    public Task RemoveItems(Item item);
}