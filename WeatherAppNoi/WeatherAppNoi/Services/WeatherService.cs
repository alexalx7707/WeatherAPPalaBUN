using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WeatherAppNoi.Models;

namespace WeatherAppNoi.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5/";
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _apiKey = "83e780119d9072fb1cb275bdc37227f2";
            _cache = cache;
        }

        public async Task<WeatherData> GetCurrentWeatherAsync(string cityName)
        {
            string normalizedCityName = cityName.Trim().ToLowerInvariant();
            string cacheKey = $"weather_{normalizedCityName}";

            if (_cache.TryGetValue(cacheKey, out WeatherData cachedData))
            {
                Console.WriteLine($"Cache hit for {cityName}");
                return cachedData;
            }

            Console.WriteLine($"Cache miss for {cityName}, fetching from API");

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}weather?q={cityName}&units=metric&appid={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // For debugging - log the raw JSON to see what's coming back
                Console.WriteLine($"API Response: {content}");

                var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var weatherData = MapToWeatherData(weatherResponse);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(100));

                _cache.Set(cacheKey, weatherData, cacheOptions);

                return weatherData;
            }
            catch (Exception ex)
            {
                // Log the exception here
                throw new Exception($"Error fetching weather data: {ex.Message}", ex);
            }
        }

        private WeatherData MapToWeatherData(WeatherApiResponse response)
        {

            // Create a DateTimeOffset from the UTC timestamp
            var utcTime = DateTimeOffset.FromUnixTimeSeconds(response.Dt);

            // Apply the timezone offset (converting from seconds to hours)
            var localTime = utcTime.ToOffset(TimeSpan.FromSeconds(response.Timezone));

            return new WeatherData
            {
                CityName = response.Name,
                Country = response.Sys.Country,
                Temperature = Math.Round(response.Main.Temp, 1),
                FeelsLike = Math.Round(response.Main.FeelsLike, 1),
                Humidity = response.Main.Humidity,
                Pressure = response.Main.Pressure,
                WindSpeed = response.Wind.Speed,
                WeatherDescription = response.Weather[0].Description,
                WeatherIcon = response.Weather[0].Icon,
                TimeStamp = localTime.DateTime
            };
        }

        // Response classes for JSON deserialization
        private class WeatherApiResponse
        {
            public MainData Main { get; set; }
            public WeatherItem[] Weather { get; set; }
            public WindData Wind { get; set; }
            public string Name { get; set; }
            public SysData Sys { get; set; }
            public long Dt { get; set; }
            public int Timezone { get; set; } // Timezone shift from UTC in seconds
            public CoordData Coord { get; set; }
        }

        private class MainData
        {
            public double Temp { get; set; }

            [JsonPropertyName("feels_like")]
            public double FeelsLike { get; set; }

            public int Humidity { get; set; }
            public int Pressure { get; set; }
            [JsonPropertyName("temp_max")]
            public double TempMax { get; set; }
            [JsonPropertyName("temp_min")]
            public double TempMin { get; set; }
        }

        private class WeatherItem
        {
            public string Description { get; set; }
            public string Icon { get; set; }
        }

        private class WindData
        {
            public double Speed { get; set; }
        }

        private class SysData
        {
            public string Country { get; set; }
        }

        public async Task<ForecastData> GetForecastAsync(string cityName)
        {
            string normalizedCityName = cityName.Trim().ToLowerInvariant();
            string cacheKey = $"forecast_{normalizedCityName}";

            if (_cache.TryGetValue(cacheKey, out ForecastData cachedData))
            {
                Console.WriteLine($"Cache hit for forecast {cityName}");
                return cachedData;
            }

            Console.WriteLine($"Cache miss for forecast {cityName}, fetching from API");

            try
            {
                // Use the 5-day/3-hour forecast API instead of One Call
                var response = await _httpClient.GetAsync($"{_baseUrl}forecast?q={cityName}&units=metric&appid={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // For debugging - log the raw JSON
                Console.WriteLine($"Forecast API Response: {content}");

                var forecastResponse = JsonSerializer.Deserialize<Forecast5DayResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var forecastData = MapToForecastDataFrom5Day(forecastResponse);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(100)); // Forecasts can be cached longer

                _cache.Set(cacheKey, forecastData, cacheOptions);

                return forecastData;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error fetching forecast data: {ex.Message}", ex);
            }
        }


        private ForecastData MapToForecastDataFrom5Day(Forecast5DayResponse response)
        {
            var forecastData = new ForecastData
            {
                CityName = response.City.Name,
                Country = response.City.Country
            };

            // Group forecast data by day - taking the data around noon for each day
            var forecastsByDay = response.List
                .GroupBy(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).DateTime.Date)
                .Take(7) // Take 7 days at most
                .ToList();

            foreach (var dayGroup in forecastsByDay)
            {
                // Get forecast item closest to noon for this day
                var dayForecast = dayGroup
                    .OrderBy(f => Math.Abs(DateTimeOffset.FromUnixTimeSeconds(f.Dt).DateTime.Hour - 12))
                    .First();

                var date = DateTimeOffset.FromUnixTimeSeconds(dayForecast.Dt).DateTime;

                var dailyForecast = new DailyForecast
                {
                    Date = date,
                    TempMax = Math.Round(dayGroup.Max(f => f.Main.TempMax), 1),
                    TempMin = Math.Round(dayGroup.Min(f => f.Main.TempMin), 1),
                    FeelsLike = Math.Round(dayForecast.Main.FeelsLike, 1),
                    Humidity = dayForecast.Main.Humidity,
                    WindSpeed = dayForecast.Wind.Speed,
                    Pressure = dayForecast.Main.Pressure,
                    Precipitation = (int)(dayForecast.Pop * 100),
                    WeatherDescription = dayForecast.Weather[0].Description,
                    WeatherIcon = dayForecast.Weather[0].Icon
                };

                forecastData.DailyForecasts.Add(dailyForecast);
            }

            return forecastData;
        }


        private class Forecast5DayResponse
        {
            public List<ForecastItem> List { get; set; }
            public CityData City { get; set; }
        }

        private class DailyData
        {
            public long Dt { get; set; }
            public TempData Temp { get; set; }
            public FeelsLikeData FeelsLike { get; set; }
            public int Humidity { get; set; }
            public double WindSpeed { get; set; }
            public int Pressure { get; set; }
            public double Pop { get; set; } // Probability of precipitation (0-1)
            public List<WeatherItem> Weather { get; set; }
        }

        private class ForecastItem
        {
            public long Dt { get; set; }
            public MainData Main { get; set; }
            public List<WeatherItem> Weather { get; set; }
            public CloudData Clouds { get; set; }
            public WindData Wind { get; set; }
            public double Pop { get; set; } // Probability of precipitation
            public string DtTxt { get; set; } // Date/time text
        }

        private class CityData
        {
            public string Name { get; set; }
            public string Country { get; set; }
            public long Sunrise { get; set; }
            public long Sunset { get; set; }
        }

        private class TempData
        {
            public double Min { get; set; }
            public double Max { get; set; }
            public double Day { get; set; }
            public double Night { get; set; }
        }

        private class FeelsLikeData
        {
            public double Day { get; set; }
            public double Night { get; set; }
        }

        private class CloudData
        {
            public int All { get; set; } // Cloudiness percentage
        }

        private class CoordData
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }
    }
}
