using ItemHub.Models.OnlyItem;
using ItemHub.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ItemHub.Data
{
    public class DataBaseContext(DbContextOptions<DataBaseContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}
