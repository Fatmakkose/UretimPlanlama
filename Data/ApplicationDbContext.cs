using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UretimPlanlama.Models;

namespace UretimPlanlama.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<Fabricator> Fabricators { get; set; }
        public DbSet<ColorDef> ColorDefs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Accessory> Accessories { get; set; }
    }
}
