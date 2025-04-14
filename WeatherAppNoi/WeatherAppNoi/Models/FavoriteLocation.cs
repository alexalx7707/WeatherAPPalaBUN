namespace WeatherAppNoi.Models
{
    public class FavoriteLocation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public Location Location { get; set; } = null!;
    }
}
