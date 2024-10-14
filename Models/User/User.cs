using System.ComponentModel.DataAnnotations;
using ItemHub.Models.OnlyItem;

namespace ItemHub.Models.User
{
    public class User
    {
        [Key]
        public Guid Id { get; init; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public byte[] Salt { get; set; }
        public string Avatar { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public DateTime DateTimeCreateAccount { get; set; }
        
        public List<string> Roles { get; set; } = [];
        public List<Item> CustomItems { get; set; } = []; /*Для Продавца*/
        public List<Guid> FavoritedItemsId { get; set; } = []; /*Для Покупателя*/

        public User(string name, string login, string email, string hashedPassword, byte[] salt, string avatar)
        {
            Id = Guid.NewGuid();
            Name = name;
            Login = login;
            Email = email;
            HashedPassword = hashedPassword;
            Salt = salt;
            Avatar = avatar;
            DateTimeCreateAccount = DateTime.UtcNow;
        }

        public void AddRoles(string[] roles) => Roles.AddRange(roles);
    }
}
