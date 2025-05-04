using Microsoft.AspNetCore.Http;
using System;

namespace WeatherAppNoi.Services
{
    public class ThemeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CookieThemeKey = "theme_preference";

        public ThemeService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTheme()
        {
            // Check for user preference in cookie first
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(CookieThemeKey, out string cookieTheme))
            {
                return cookieTheme;
            }

            // Otherwise, decide based on time of day
            var currentHour = DateTime.Now.Hour;

            // Default: 7 AM to 7 PM use light theme, otherwise use dark theme
            return (currentHour >= 7 && currentHour < 19) ? "light" : "dark";
        }

        public void SetThemePreference(string theme)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append(CookieThemeKey, theme, cookieOptions);
        }
    }
}
