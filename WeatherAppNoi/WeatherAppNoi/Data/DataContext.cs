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
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
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
        }

    }
}
