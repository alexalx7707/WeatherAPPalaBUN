using Microsoft.AspNetCore.Mvc;
using WeatherAppNoi.Services;

namespace WeatherAppNoi.ViewComponents
{
    public class ThemeSwitcherViewComponent : ViewComponent
    {
        private readonly ThemeService _themeService;

        public ThemeSwitcherViewComponent(ThemeService themeService)
        {
            _themeService = themeService;
        }

        public IViewComponentResult Invoke()
        {
            var theme = _themeService.GetCurrentTheme();
            return View("Default", theme);
        }
    }
}
