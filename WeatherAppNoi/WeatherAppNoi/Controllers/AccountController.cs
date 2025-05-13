using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WeatherAppNoi.Models;
using WeatherAppNoi.Services;

namespace WeatherAppNoi.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly WeatherService _weatherService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, WeatherService weatherService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _weatherService = weatherService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Clear any existing model state to start fresh
            ModelState.Clear();
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // If we're coming from the Login view with tabs, handle differently
            if (Request.Headers.Referer.ToString().Contains("/Account/Login"))
            {
                return await HandleRegisterFromLoginTab(model);
            }

            if (!ModelState.IsValid) return View(model);

            // Check if user already exists with this email
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists. Please try logging in instead.");
                return View(model);
            }

            var user = new User
            {
                UserName = model.FullName, // Keep full name as username for login
                Email = model.Email,
                Settings = new UserSettings
                {
                    TemperatureUnit = 1, // Celsius
                    WindSpeedUnit = 1,   // m/s
                    PressureUnit = 1,    // hPa
                    DistanceUnit = 1,    // km
                    DarkMode = false
                }
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateEmail")
                {
                    ModelState.AddModelError("Email", "An account with this email already exists. Please try logging in instead.");
                }
                else
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        private async Task<IActionResult> HandleRegisterFromLoginTab(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Set TempData to show the signup tab
                TempData["ShowRegisterTab"] = true;
                var loginModel = new LoginViewModel();

                // Copy error messages to ViewData for display
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }

                return View("Login", loginModel);
            }

            // Check if user already exists with this email
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ShowRegisterTab"] = true;
                var loginModel = new LoginViewModel();
                ModelState.AddModelError("", "An account with this email already exists. Please try logging in instead.");
                return View("Login", loginModel);
            }

            try
            {
                var user = new User
                {
                    UserName = model.FullName,
                    Email = model.Email,
                    Settings = new UserSettings
                    {
                        TemperatureUnit = 1,
                        WindSpeedUnit = 1,
                        PressureUnit = 1,
                        DistanceUnit = 1,
                        DarkMode = false
                    }
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }

                // If registration failed, show errors on login page
                TempData["ShowRegisterTab"] = true;
                var loginModelWithErrors = new LoginViewModel();

                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                    {
                        ModelState.AddModelError("", "An account with this email already exists. Please try logging in instead.");
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View("Login", loginModelWithErrors);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                TempData["ShowRegisterTab"] = true;
                var loginModel = new LoginViewModel();
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View("Login", loginModel);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // In a real application, you would send this token via email
            // For now, we'll just redirect to a confirmation page
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = model.Email;

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
    }
}