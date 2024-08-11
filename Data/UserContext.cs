using ItemHub.Models;
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
