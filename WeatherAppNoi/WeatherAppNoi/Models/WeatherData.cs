using System;

namespace WeatherAppNoi.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public string CityName { get; set; }
        public string Country { get; set; }
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int Pressure { get; set; }
        public string WeatherDescription { get; set; }
        public string WeatherIcon { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}