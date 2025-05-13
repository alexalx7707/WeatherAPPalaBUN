namespace WeatherAppNoi.Models
{
    public class WeatherAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int LocationId { get; set; }

        public Location Location { get; set; } = null!;

        public string AlertType { get; set; } = string.Empty!;

        public string Description { get; set; } = string.Empty!;

        public bool IsActive { get; set; }
    }
}