using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WeatherAppNoi.Models;

namespace WeatherAppNoi.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5/";

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = "83e780119d9072fb1cb275bdc37227f2";
        }

        public async Task<WeatherData> GetCurrentWeatherAsync(string cityName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}weather?q={cityName}&units=metric&appid={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonSerializer.Deserialize<WeatherApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return MapToWeatherData(weatherResponse);
            }
            catch (Exception ex)
            {
                // Log the exception here
                throw new Exception($"Error fetching weather data: {ex.Message}", ex);
            }
        }

        private WeatherData MapToWeatherData(WeatherApiResponse response)
        {
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
                TimeStamp = DateTimeOffset.FromUnixTimeSeconds(response.Dt).DateTime
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
        }

        private class MainData
        {
            public double Temp { get; set; }
            public double FeelsLike { get; set; }
            public int Humidity { get; set; }
            public int Pressure { get; set; }
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
    }
}