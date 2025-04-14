using Microsoft.AspNetCore.Identity;

namespace WeatherAppNoi.Models
{
    public class User:IdentityUser<int>
    {
        public List<FavoriteLocation> FavoriteLocation { get; set; } = [];

        public UserSettings Settings { get; set; } = null!;

        public List<WeatherAlert> WeatherAlerts { get; set; } = [];

        public List<UserRole> UserRoles { get; set; } = [];
    }
}
