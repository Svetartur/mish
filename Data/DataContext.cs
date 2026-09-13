using Microsoft.EntityFrameworkCore;

namespace ASP_P42.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entities.UserData> UsersData { get; set; }
        public DbSet<Entities.UserRole> UserRoles { get; set; }
        public DbSet<Entities.UserAccess> UserAccesses { get; set; }
        public DbSet<Entities.AuthJournal> AuthJournals { get; set; }

        public DbSet<Entities.ProductGroup> ProductGroups { get; set; }
        public DbSet<Entities.Product> Products { get; set; }
        public DbSet<Entities.ProductVersion> ProductVersions { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.ProductGroup>()
                .HasIndex(p => p.Slug)
                .IsUnique();
            
            modelBuilder.Entity<Entities.Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();
            
            modelBuilder.Entity<Entities.ProductVersion>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Entities.Product>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Products)
                .HasForeignKey(p => p.GroupId);

            modelBuilder.Entity<Entities.ProductVersion>()
                .HasOne(p => p.Product)
                .WithMany(g => g.Versions);

            modelBuilder.Entity<Entities.ProductGroup>()
                .HasOne(p => p.ParentGroup)
                .WithMany(g => g.Children)
                .HasForeignKey(p => p.ParentId);

            modelBuilder.Entity<Entities.UserAccess>()
                .HasIndex(ua => ua.Login)
                .IsUnique();

            modelBuilder.Entity<Entities.UserAccess>()
                .HasOne(ua => ua.UserData)
                .WithMany(ud => ud.Accesses)
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<Entities.UserAccess>()
                .HasOne(ua => ua.UserRole)
                .WithMany()
                .HasForeignKey(ua => ua.RoleId);

            modelBuilder.Entity<Entities.UserRole>()
                .HasData([
                    new() {
                        Id = Guid.Parse("ACB35324-7B84-4E3B-9A26-00AAD72A600C"),
                        Name = "Admin",
                        Description = "Кореневий адміністратор",
                        CreateLevel = 10,
                        ReadLevel = 10,
                        UpdateLevel = 10,
                        DeleteLevel = 10,
                    },
                    new() {
                        Id = Guid.Parse("702D05C2-FCD0-4C1D-B0BB-2AEB4B98F91A"),
                        Name = "User",
                        Description = "Самозареєстрований користувач",
                        CreateLevel = 0,
                        ReadLevel = 0,
                        UpdateLevel = 0,
                        DeleteLevel = 0,
                    }
                ]);
            modelBuilder.Entity<Entities.UserData>()
                .HasData([
                    new() {
                        Id = Guid.Parse("190052CA-F844-498A-A05F-1D4BA2ADC0E8"),
                        FullName = "Адміністратор Системи",
                        Birthdate = DateTime.UnixEpoch,
                        Email = "CHANGE@ME",
                        Phone = "CHANGE_ME",
                        RegisteredAt = DateTime.UnixEpoch,
                    }
                ]);
            modelBuilder.Entity<Entities.UserAccess>()
                .HasData([
                    new() {
                        Id = Guid.Parse("96DCBBBA-9AEE-44A2-8835-72DFE4E1A710"),
                        UserId= Guid.Parse("190052CA-F844-498A-A05F-1D4BA2ADC0E8"),
                        RoleId = Guid.Parse("ACB35324-7B84-4E3B-9A26-00AAD72A600C"),
                        Login = "Admin",
                        Salt = "96DCBBBA-9AEE-44A2-8835-72DFE4E1A710",
                        Dk = "FCB57CECE720632FDBB68958CF953E46",
                    }
                ]);
        }
    }
}