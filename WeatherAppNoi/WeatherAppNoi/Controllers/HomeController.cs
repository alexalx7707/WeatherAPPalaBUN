using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherAppNoi.Models;

namespace WeatherAPP2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Home";
            ViewBag.ActivePage = "Home";
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.Title = "Login";
            ViewBag.ActivePage = "Login";
            return View();
        }

        public IActionResult Current()
        {
            ViewBag.Title = "Current Weather";
            ViewBag.ActivePage = "Current";
            return View();
        }

        public IActionResult Forecast()
        {
            ViewBag.Title = "Forecast";
            ViewBag.ActivePage = "Forecast";
            return View();
        }

        public IActionResult Profile()
        {
            ViewBag.Title = "My Profile";
            ViewBag.ActivePage = "Profile";

            // Create dummy data directly in ViewBag
            ViewBag.UserName = "John Doe";
            ViewBag.FavoriteLocations = new[]
            {
            new {
                City = "New York, NY",
                Temperature = "72°F",
                Condition = "Partly Cloudy",
                HighTemperature = "77°F",
                LowTemperature = "65°F"
            },
            new {
                City = "Bucharest, Romania",
                Temperature = "58°F",
                Condition = "Mostly Cloudy",
                HighTemperature = "62°F",
                LowTemperature = "53°F"
            }
        };

            return View();
        }

        public IActionResult About()
        {
            ViewBag.Title = "About";
            ViewBag.ActivePage = "About";
            return View();
        }
    }
}
