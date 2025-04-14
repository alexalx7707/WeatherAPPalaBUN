using Microsoft.AspNetCore.Identity;

namespace WeatherAppNoi.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        public Role Role { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
