using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherAppNoi.Models;
using WeatherAppNoi.Services;

namespace WeatherAppNoi.Controllers
{
    public class HomeController : Controller
    {
        private readonly WeatherService _weatherService;

        public HomeController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetWeather(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var weatherData = await _weatherService.GetCurrentWeatherAsync(cityName);
                ViewBag.WeatherData = weatherData;
                return View("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                ViewBag.ErrorMessage = $"Could not retrieve weather data: {ex.Message}";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult SetThemePreference(string theme)
        {
            var themeService = HttpContext.RequestServices.GetRequiredService<ThemeService>();
            themeService.SetThemePreference(theme);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CurrentWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return View();
            }

            try
            {
                var weatherData = await _weatherService.GetCurrentWeatherAsync(city);
                return View(weatherData);
            }
            catch (Exception ex)
            {
                // Log the exception
                ViewBag.ErrorMessage = $"Could not retrieve weather data: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Forecast()
        {
            // Implementation for forecast view (would be similar to CurrentWeather)
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Current()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }
    }
}