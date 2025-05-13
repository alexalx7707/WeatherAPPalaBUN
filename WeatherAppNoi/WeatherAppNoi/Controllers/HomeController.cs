using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeatherAppNoi.Models;
using WeatherAppNoi.Services;
using WeatherAppNoi.Data;

namespace WeatherAppNoi.Controllers
{
    public class HomeController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public HomeController(WeatherService weatherService, UserManager<User> userManager, DataContext context)
        {
            _weatherService = weatherService;
            _userManager = userManager;
            _context = context;
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
        public async Task<IActionResult> Forecast(string location)
        {

            if (string.IsNullOrWhiteSpace(location))
            {
                return View();
            }

            try
            {
                var forecastData = await _weatherService.GetForecastAsync(location);
                return View(forecastData);
            }
            catch (Exception ex)
            {
                // Log the exception
                ViewBag.ErrorMessage = $"Could not retrieve forecast data: {ex.Message}";
                return View();
            }
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
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Current()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user's favorite locations with current weather
            var favoriteLocations = await _context.FavoriteLocation
                .Include(fl => fl.Location)
                .Where(fl => fl.UserId == user.Id)
                .ToListAsync();

            // Create a strongly typed list
            var favoriteWeatherData = new List<FavoriteLocationWeatherViewModel>();

            foreach (var favorite in favoriteLocations)
            {
                try
                {
                    var weather = await _weatherService.GetCurrentWeatherAsync(favorite.Location.City);
                    favoriteWeatherData.Add(new FavoriteLocationWeatherViewModel
                    {
                        Id = favorite.Id,
                        City = favorite.Location.City,
                        Country = favorite.Location.Country,
                        Temperature = $"{weather.Temperature}°C",
                        Condition = weather.WeatherDescription,
                        HighTemperature = $"{weather.Temperature + 3}°C", // Approximation
                        LowTemperature = $"{weather.Temperature - 5}°C"   // Approximation
                    });
                }
                catch
                {
                    // If weather data fails to load, add without temperature
                    favoriteWeatherData.Add(new FavoriteLocationWeatherViewModel
                    {
                        Id = favorite.Id,
                        City = favorite.Location.City,
                        Country = favorite.Location.Country,
                        Temperature = "N/A",
                        Condition = "Data unavailable",
                        HighTemperature = "N/A",
                        LowTemperature = "N/A"
                    });
                }
            }

            ViewBag.UserName = user.UserName;
            ViewBag.FavoriteLocations = favoriteWeatherData;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddFavoriteLocation(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return BadRequest("City name is required");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                // Check if location exists
                var existingLocation = await _context.FavoriteLocation
                    .Include(fl => fl.Location)
                    .FirstOrDefaultAsync(fl => fl.UserId == user.Id &&
                                              fl.Location.City.ToLower() == cityName.ToLower());

                if (existingLocation != null)
                {
                    return Json(new { success = false, message = "Location already in favorites" });
                }

                // Get weather data to validate location
                var weatherData = await _weatherService.GetCurrentWeatherAsync(cityName);

                // Create new location and favorite
                var location = new Location
                {
                    City = weatherData.CityName,
                    Country = weatherData.Country,
                    Region = "", // OpenWeatherMap doesn't provide region
                    Latitude = 0, // Would need to be fetched from coordinates if needed
                    Longitude = 0
                };

                _context.Location.Add(location);
                await _context.SaveChangesAsync();

                var favorite = new FavoriteLocation
                {
                    UserId = user.Id,
                    LocationId = location.Id // Fixed: Use LocationId instead of Location
                };

                _context.FavoriteLocation.Add(favorite);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Location added to favorites" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFavoriteLocation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var favorite = await _context.FavoriteLocation
                .FirstOrDefaultAsync(fl => fl.Id == id && fl.UserId == user.Id);

            if (favorite == null)
            {
                return NotFound();
            }

            _context.FavoriteLocation.Remove(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Location removed from favorites" });
        }
    }
}