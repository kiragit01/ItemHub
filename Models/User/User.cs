using System.ComponentModel.DataAnnotations;
using ItemHub.Models.OnlyItem;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Models.User
{
    public class User(string name, string login, string email, string hashedPassword, byte[] salt, string avatar)
    {
        [Key]
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; set; } = name;
        public string Login { get; set; } = login;
        public string Email { get; set; } = email;
        public string HashedPassword { get; set; } = hashedPassword;
        public byte[] Salt { get; set; } = salt;
        public string Avatar { get; set; } = avatar;
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public DateTime DateTimeCreateAccount { get; set; } = DateTime.UtcNow;

        public List<string> Roles { get; set; } = [];
        public List<Item> CustomItems { get; set; } = []; /*Для Продавца*/
        public List<Guid> FavoritedItemsId { get; set; } = []; /*Для Покупателя*/

        public void UpdateDataUser(string name, string login, string email, string avatar, string? description, string? phone)
        {
            Name = name;
            Login = login;
            Email = email;
            Avatar = avatar;
            Description = description;
            Phone = phone;
        }

        public void AddRoles(string[] roles) => Roles.AddRange(roles);
    }
}
