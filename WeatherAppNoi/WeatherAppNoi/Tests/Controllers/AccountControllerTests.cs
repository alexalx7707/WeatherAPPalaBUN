using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using WeatherAppNoi.Controllers;
using WeatherAppNoi.Models;
using WeatherAppNoi.Services;

namespace WeatherAppNoi.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<WeatherService> _mockWeatherService;
        private AccountController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Set up UserManager mock
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object,
                null, null, null, null, null, null, null, null);

            // Set up SignInManager mock - this requires several dependencies
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<User>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<User>>();

            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                options.Object,
                logger.Object,
                schemes.Object,
                confirmation.Object);

            var mockHttpClient = new Mock<HttpClient>(); // Note: HttpClient isn't mockable this way
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCache = new Mock<IMemoryCache>();

            _mockWeatherService = new Mock<WeatherService>(
                mockHttpClient.Object,
                mockConfiguration.Object,
                mockCache.Object);

            // Create the controller with mocked dependencies
            _controller = new AccountController(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockWeatherService.Object);

            // Setup controller context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [TestMethod]
        public void Login_Get_ReturnsViewWithEmptyModel()
        {
            // Act
            var result = _controller.Login() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(LoginViewModel));
            Assert.AreEqual(string.Empty, ((LoginViewModel)result.Model).Email);
            Assert.AreEqual(string.Empty, ((LoginViewModel)result.Model).Password);
        }

        [TestMethod]
        public async Task Login_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "invalid-email",
                Password = "password"
            };
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Login(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(LoginViewModel));
            Assert.AreSame(model, result.Model);
        }

        [TestMethod]
        public async Task Login_Post_UserNotFound_AddsModelError_ReturnsView()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "nonexistent@example.com",
                Password = "password"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(LoginViewModel));
            Assert.AreSame(model, result.Model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public async Task Login_Post_PasswordSignInFails_AddsModelError_ReturnsView()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "wrong-password",
                RememberMe = false
            };

            var user = new User { Email = model.Email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(LoginViewModel));
            Assert.AreSame(model, result.Model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public async Task Login_Post_ValidCredentials_RedirectsToHome()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "correct-password",
                RememberMe = true
            };

            var user = new User { Email = model.Email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(user);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(user, model.Password, model.RememberMe, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);

            // Verify that the UserManager and SignInManager were called with correct parameters
            _mockUserManager.Verify(x => x.FindByEmailAsync(model.Email), Times.Once);
            _mockSignInManager.Verify(x =>
                x.PasswordSignInAsync(user, model.Password, model.RememberMe, false), Times.Once);
        }
    }
}