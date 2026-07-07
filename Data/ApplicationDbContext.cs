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
        public DbSet<Brand> Brands { get; set; }

        // Cari Yönetimi
        public DbSet<CariHesap> CariHesaplar { get; set; }
        public DbSet<CariHareket> CariHareketler { get; set; }

        // Stok Yönetimi
        public DbSet<StokKarti> StokKartlari { get; set; }
        public DbSet<StokVaryant> StokVaryantlar { get; set; }
        public DbSet<StokHareket> StokHareketler { get; set; }
        public DbSet<OrderMaterial> OrderMaterials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CariHesap → CariHareket (1:N)
            modelBuilder.Entity<CariHareket>()
                .HasOne(h => h.CariHesap)
                .WithMany(c => c.Hareketler)
                .HasForeignKey(h => h.CariHesapId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CariHareket>()
                .HasOne(h => h.Order)
                .WithMany()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // StokKarti → StokHareket (1:N)
            modelBuilder.Entity<StokHareket>()
                .HasOne(h => h.StokKarti)
                .WithMany(s => s.Hareketler)
                .HasForeignKey(h => h.StokKartiId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StokHareket>()
                .HasOne(h => h.Order)
                .WithMany()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order → OrderMaterial (1:N)
            modelBuilder.Entity<OrderMaterial>()
                .HasOne(m => m.Order)
                .WithMany(o => o.OrderMaterials)
                .HasForeignKey(m => m.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // İndeksler
            modelBuilder.Entity<CariHesap>()
                .HasIndex(c => c.HesapKodu)
                .IsUnique();

            modelBuilder.Entity<StokKarti>()
                .HasIndex(s => s.StokKodu)
                .IsUnique();
        }
    }
}
