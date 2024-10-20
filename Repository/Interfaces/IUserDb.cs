using ItemHub.Models.User;
using ItemHub.Services;

namespace ItemHub.Repository.Interfaces;

public interface IUserDb
{
    public Task AddUser(User user);
    public Task<User?> GetUser(string? login);
    public Task<ResponseMessage> CheckUser(string email, string login);
    public Task<User?> LogIn(string login, string password);
    public Task UpdateUser(User user);
    public Task DeleteUser(User user);
}