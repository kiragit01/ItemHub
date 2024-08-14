using System.ComponentModel.DataAnnotations;
using ItemHub.Models.OnlyItem;

namespace ItemHub.Models.User
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }

        public List<string> Roles { get; set; } = new();
        public List<Item> Items { get; set; } = new();

        public User(string login, string password, string name, string email, int age)
        {
            Id = Guid.NewGuid();
            Login = login;
            Password = password;
            Name = name;
            Email = email;
            Age = age;
        }
        public User(string login, string password, string name, string email, int age, string[] roles) : this(login, password, name, email, age)
        {
            Roles.AddRange(roles);
        }

        public void AddRoles(string[] roles) => Roles.AddRange(roles);

        public void AddItems(Item item) => Items.Add(item);

    }
}
