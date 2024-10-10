using System.ComponentModel.DataAnnotations;
using ItemHub.Models.OnlyItem;

namespace ItemHub.Models.User
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public string Avatar { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        
        public List<string> Roles { get; set; } = [];
        public List<Item> Items { get; set; } = []; /*Для Продавца*/
        public List<Guid> FavoritedItemsId { get; set; } = []; /*Для Покупателя*/

        public User(string login, string hashedPassword, string name, string email)
        {
            Id = Guid.NewGuid();
            Login = login;
            HashedPassword = hashedPassword;
            Name = name;
            Email = email;
        }
        public User(string login, string password, string name, string email, string[] roles) : this(login, password, name, email)
        {
            Roles.AddRange(roles);
        }

        public void AddRoles(string[] roles) => Roles.AddRange(roles);
    }
}
