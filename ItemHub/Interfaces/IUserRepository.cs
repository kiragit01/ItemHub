using ItemHub.Models.User;
using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IUserRepository
{
    public Task AddUserAsync(User user);
    public Task<User?> GetUserAsync(string? login = null);
    public Task<ResponseMessage> CheckUserAsync(string email, string login);
    public Task<User?> LogInAsync(string login, string password);
    public Task UpdateUserAsync(User user);
    public Task DeleteUserAsync(User user);
}