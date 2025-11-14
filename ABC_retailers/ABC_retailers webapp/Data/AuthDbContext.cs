using System.Collections.Generic;
using ABC_retailers.Models;
using Microsoft.EntityFrameworkCore;

namespace ABC_retailers.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();



        // ✅ Add these two:
        public DbSet<Cart> Cart => Set<Cart>();
        public DbSet<Order> Orders => Set<Order>();
    }
}
