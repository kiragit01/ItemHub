using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ItemHub.Data
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
    }
}
