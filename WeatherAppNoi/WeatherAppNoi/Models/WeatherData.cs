namespace WeatherAppNoi.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public int Timestamp { get; set; }

        public string Condition { get; set; } = string.Empty;

        public int Temperature { get; set; }

        public int FeelsLike { get; set; }

        public int Humidity { get; set; }

        public int WindSpeed { get; set; }

        public int Pressure { get; set; }

        public int Visibility { get; set; }

        public int UvIndex { get; set; }
    }
}
