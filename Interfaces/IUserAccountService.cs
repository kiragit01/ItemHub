using ItemHub.Models.Auth;
using ItemHub.Models.User;
using ItemHub.Utilities;

namespace ItemHub.Interfaces;

public interface IUserAccountService
{
    public Task<User?> GetUser(string? login = null);
    public Task<string?> EditAccount(EditAccountModel model);
    public Task<string?> UpdatePassword(UpdatePasswordModel model);
    public Task<ResponseMessage> DeleteAccount();
}