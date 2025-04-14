namespace WeatherAppNoi.Models
{
    public class UserSettings

    {
        public int Id { get; set; }
        public int TemperatureUnit { get; set; }

        public int WindSpeedUnit { get; set; }

        public int PressureUnit { get; set; }

        public int DistanceUnit { get; set; }   

        public bool DarkMode { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
