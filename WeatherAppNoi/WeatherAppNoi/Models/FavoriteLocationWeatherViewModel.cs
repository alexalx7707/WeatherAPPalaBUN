namespace WeatherAppNoi.Models
{
    public class FavoriteLocationWeatherViewModel
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Temperature { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string HighTemperature { get; set; } = string.Empty;
        public string LowTemperature { get; set; } = string.Empty;
    }
}