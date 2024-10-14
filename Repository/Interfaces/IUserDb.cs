using ItemHub.Models.User;

namespace ItemHub.Repository.Interfaces;

public interface IUserDb
{
    public Task AddUser(User user);
    public Task<User?> GetUser(string? login);
    public Task<DebugMessage> CheckUser(string email, string login);
    public Task<User?> SingIn(string login, string password);
    public Task UpdateUser(User user);
    public Task DeleteUser(User user);
}