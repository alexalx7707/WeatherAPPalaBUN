using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using WeatherAppNoi.Models;
using WeatherAppNoi.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace WeatherAppNoi.Tests
{
    [TestClass]
    public class WeatherServiceTests
    {
        // Test 1: When Cache Hit
        [TestMethod]
        public async Task GetCurrentWeatherAsync_WhenCacheHit_ReturnsCachedData()
        {
            // Arrange
            string cityName = "london";
            var weatherData = new WeatherData
            {
                CityName = "London",
                Country = "GB",
                Temperature = 15.5,
                FeelsLike = 14.2,
                Humidity = 70,
                Pressure = 1012,
                WindSpeed = 5.2,
                WeatherDescription = "cloudy",
                WeatherIcon = "04d",
                TimeStamp = DateTime.Now
            };

            // Create mock cache with data
            var mockCache = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();

            object retrievedValue = weatherData;
            mockCache
                .Setup(x => x.TryGetValue(It.Is<string>(s => s == $"weather_{cityName}"), out retrievedValue))
                .Returns(true);

            // Mock HTTP client (should not be called)
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage())
                .Verifiable("HTTP request should not be made when cache hit");

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var mockConfig = new Mock<IConfiguration>();

            // Create the service
            var weatherService = new WeatherService(httpClient, mockConfig.Object, mockCache.Object);

            // Act
            var result = await weatherService.GetCurrentWeatherAsync(cityName);

            // Assert
            Assert.IsNotNull(result);


            // With these:
            Assert.AreEqual("London", result.CityName);
            Assert.AreEqual("GB", result.Country);
            Assert.AreEqual(15.5, result.Temperature);


            mockCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Never,
                "Cache entry should not be created on cache hit");

            // Verify HTTP was NOT called
            mockHttpHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        // Test 2: When Cache Miss
        [TestMethod]
        public async Task GetCurrentWeatherAsync_WhenCacheMiss_FetchesDataAndCachesIt()
        {
            // Arrange
            string cityName = "paris";
            WeatherData nullData = null;

            // API response
            var apiResponse = @"{
                ""coord"":{""lon"":2.3488,""lat"":48.8534},
                ""weather"":[{""id"":800,""main"":""Clear"",""description"":""clear sky"",""icon"":""01d""}],
                ""base"":""stations"",
                ""main"":{""temp"":18.2,""feels_like"":17.8,""temp_min"":16.1,""temp_max"":20.1,""pressure"":1015,""humidity"":65},
                ""visibility"":10000,
                ""wind"":{""speed"":3.1,""deg"":250},
                ""clouds"":{""all"":0},
                ""dt"":1621345678,
                ""sys"":{""type"":2,""id"":2041230,""country"":""FR"",""sunrise"":1621348883,""sunset"":1621398213},
                ""timezone"":7200,
                ""id"":2988507,
                ""name"":""Paris"",
                ""cod"":200
            }";

            // Create mock cache with no data
            var mockCache = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();

            // Setup cache miss
            object nullValue = null;
            mockCache
                .Setup(x => x.TryGetValue(It.Is<string>(s => s == $"weather_{cityName}"), out nullValue))
                .Returns(false);

            // Setup cache set
            mockCache
                .Setup(x => x.CreateEntry(It.Is<string>(s => s == $"weather_{cityName}")))
                .Returns(mockCacheEntry.Object);

            // Mock HTTP client with response
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(apiResponse)
                })
                .Verifiable("HTTP request should be made when cache miss");

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var mockConfig = new Mock<IConfiguration>();

            // Create the service
            var weatherService = new WeatherService(httpClient, mockConfig.Object, mockCache.Object);

            // Act
            var result = await weatherService.GetCurrentWeatherAsync(cityName);

            // Assert
            Assert.IsNotNull(result);


            // With these:
            Assert.AreEqual("Paris", result.CityName);
            Assert.AreEqual("FR", result.Country);
            Assert.AreEqual(18.2, result.Temperature);

            // Verify HTTP was called
            mockHttpHandler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );

            // Verify data was cached
            mockCache.Verify(x => x.CreateEntry(It.Is<string>(s => s == $"weather_{cityName}")), Times.Once);
        }
    }
}
