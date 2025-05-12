using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeatherAppNoi.Models;

namespace WeatherAppNoi.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<WeatherData> WeatherData { get; set; } = null!;
        public DbSet<Location> Location { get; set; } = null!;
        public DbSet<FavoriteLocation> FavoriteLocation { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<WeatherAlert> WeatherAlert { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserRole>()
                   .HasKey(ur => new { ur.UserId, ur.RoleId });
            builder.Entity<IdentityUserLogin<int>>()
                .HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
            builder.Entity<IdentityUserToken<int>>()
                .HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            builder.Entity<IdentityRoleClaim<int>>()
                .HasKey(rc => new { rc.Id, rc.RoleId });
            builder.Entity<IdentityUserClaim<int>>()
                .HasKey(uc => new { uc.Id, uc.UserId });

            // Configure relationships
            builder.Entity<User>()
                .HasOne(u => u.Settings)
                .WithOne(s => s.User)
                .HasForeignKey<UserSettings>(s => s.UserId);

            builder.Entity<FavoriteLocation>()
                .HasOne(fl => fl.User)
                .WithMany(u => u.FavoriteLocation) // Fixed property name
                .HasForeignKey(fl => fl.UserId);

            builder.Entity<Location>()
                .HasOne(l => l.FavoriteLocation)
                .WithOne(fl => fl.Location)
                .HasForeignKey<FavoriteLocation>(fl => fl.LocationId);

            builder.Entity<WeatherAlert>()
                .HasOne(wa => wa.User)
                .WithMany(u => u.WeatherAlerts)
                .HasForeignKey(wa => wa.UserId);
        }
    }
}