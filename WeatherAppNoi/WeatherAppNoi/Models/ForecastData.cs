// WeatherAppNoi/Models/ForecastData.cs
using System;
using System.Collections.Generic;

namespace WeatherAppNoi.Models
{
    public class ForecastData
    {
        public string CityName { get; set; }
        public string Country { get; set; }
        public List<DailyForecast> DailyForecasts { get; set; }

        public ForecastData()
        {
            DailyForecasts = new List<DailyForecast>();
        }
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public double TempMax { get; set; }
        public double TempMin { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int Pressure { get; set; }
        public int Precipitation { get; set; } // Probability of precipitation in percentage
        public string WeatherDescription { get; set; }
        public string WeatherIcon { get; set; }

        // Helper property for UI display
        public string DayOfWeek => Date.DayOfWeek.ToString();

        // Helper property for date formatting
        public string FormattedDate => Date.ToString("MMMM d");
    }
}
