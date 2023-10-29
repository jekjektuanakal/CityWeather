using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CityWeather.Controllers;
using CityWeather.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CityWeather.Test
{
    [TestFixture]
    public class CountriesTests
    {
        private readonly IReadOnlyCollection<Country> countries = new List<Country>
        {
            new Country { Code = "SG", Name = "Singapore" },
            new Country { Code = "AU", Name = "Australia" },
            new Country { Code = "MY", Name = "Malaysia" },
            new Country { Code = "ID", Name = "Indonesia" }
        };

        private readonly IReadOnlyCollection<City> cities = new List<City>
        {
            new City { Name = "Changi", Country = "SG", Location = "CHG" },
            new City { Name = "Sydney", Country = "AU", Location = "SYD" },
            new City { Name = "Melbourne", Country = "AU", Location = "MEL" },
            new City { Name = "Brisbane", Country = "AU", Location = "BNE" },
            new City { Name = "Penang", Country = "MY", Location = "PNG" },
            new City { Name = "Jakarta", Country = "ID", Location = "JKT" },
            new City { Name = "Yogyakarta", Country = "ID", Location = "YGY" }
        };

        private ICountryService service;
        private CountriesController controller;

        [SetUp]
        public void Setup()
        {
            this.service = new LocalCountryService(this.countries, this.cities);
            this.controller = new CountriesController(this.service);
        }

        [Test]
        public async Task TestGetCountries_ReturnsResult()
        {
            var result = await this.controller.Get();

            Assert.GreaterOrEqual(result.Value.Count, 1);
            Assert.IsNotEmpty(result.Value[0].Code);
            Assert.IsNotEmpty(result.Value[0].Name);
        }

        [Test]
        public async Task TestGetCities_InvalidCode_CountryServiceThrows()
        {
            var result = await this.controller.Get("SGPCL");

            Assert.AreEqual(400, ((ObjectResult)result.Result).StatusCode);
        }

        [Test]
        public async Task TestGetCities_ValidCode_ReturnsCities()
        {
            var result = await this.controller.Get("AU");

            Assert.AreEqual(3, result.Value.Count);
            Assert.True(result.Value.All(city => city.Country == "AU"));
            Assert.True(result.Value.Any(city => city.Name == "Sydney"));
            Assert.True(result.Value.Any(city => city.Name == "Melbourne"));
            Assert.True(result.Value.Any(city => city.Name == "Brisbane"));
        }
    }

    [TestFixture]
    public class WeatherTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private IWeatherService service;
        private WeatherController controller;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(this.mockHttpMessageHandler.Object);
            this.service = new OpenWeatherService(this.httpClient, "1234567890");
            this.controller = new WeatherController(this.service);
        }

        [Test]
        public async Task TestGetWeather_WeatherServiceThrows()
        {
            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.Unauthorized });

            var result = await this.controller.Get("Yogyakarta");

            Assert.AreEqual(503, ((ObjectResult)result.Result).StatusCode);
        }

        [Test]
        public async Task TestGetWeather_InvalidCityName_Status400()
        {
            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound });

            var result = await this.controller.Get("Wedomartani");

            Assert.AreEqual(400, ((ObjectResult)result.Result).StatusCode);
        }

        [Test]
        [TestCase(30.0, 86.0)]
        [TestCase(25.0, 77.0)]
        [TestCase(20.0, 68.0)]
        public async Task TestGetWeather_ValidCity_WeatherServiceReturnResult(double tempCelcius, double tempFahrenheit)
        {
            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(
                            @"{
                        ""name"": ""Yogyakarta"",
                        ""dt"": 1586462400,
                        ""wind"": {
                            ""speed"": 10,
                            ""deg"": 90
                        },
                        ""visibility"": 10000,
                        ""weather"": [
                            {
                                ""description"": ""broken clouds""
                            }
                        ],
                        ""main"": {
                            ""temp"": " +
                            tempCelcius +
                            @",
                            ""pressure"": 1000,
                            ""humidity"": 100
                        }
                    }")
                    });


            var result = await controller.Get("Yogyakarta");

            Assert.AreEqual("Yogyakarta", result.Value.Location);
            Assert.That(10.0, Is.EqualTo(result.Value.Wind.Speed).Within(0.01));
            Assert.That(90.0, Is.EqualTo(result.Value.Wind.Deg).Within(0.01));
            Assert.That(10000, Is.EqualTo(result.Value.Visibility).Within(0.01));
            Assert.AreEqual("broken clouds", result.Value.SkyConditions);
            Assert.That(tempCelcius, Is.EqualTo(result.Value.TemperatureCelsius).Within(0.01));
            Assert.That(tempFahrenheit, Is.EqualTo(result.Value.TemperatureFahrenheit).Within(0.01));
            Assert.That(tempCelcius, Is.EqualTo(result.Value.DewPointCelsius).Within(0.01));
            Assert.That(tempFahrenheit, Is.EqualTo(result.Value.DewPointFahrenheit).Within(0.01));
            Assert.That(100.0, Is.EqualTo(result.Value.RelativeHumidity).Within(0.01));
            Assert.That(1000.0, Is.EqualTo(result.Value.Pressure).Within(0.01));
        }
        
        [Test]
        [TestCase(30.0, 90,  28.18)]
        [TestCase(20.0, 90, 18.31)]
        [TestCase(30.0, 60,  21.39)]
        [TestCase(20.0, 60, 12)]
        public async Task TestGetWeather_ValidCity_DewPointCases(double tempC, double humidity, double dewPointC)
        {
            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Content = new StringContent(
                            @"{
                        ""name"": ""Yogyakarta"",
                        ""dt"": 1586462400,
                        ""wind"": {
                            ""speed"": 10,
                            ""deg"": 90
                        },
                        ""visibility"": 10000,
                        ""weather"": [
                            {
                                ""description"": ""broken clouds""
                            }
                        ],
                        ""main"": {
                            ""temp"": " +
                            tempC +
                            @",
                            ""pressure"": 1000,
                            ""humidity"": " + humidity + @"
                        }
                    }")
                    });


            var result = await controller.Get("Yogyakarta");

            Assert.AreEqual("Yogyakarta", result.Value.Location);
            Assert.That(10.0, Is.EqualTo(result.Value.Wind.Speed).Within(0.01));
            Assert.That(90.0, Is.EqualTo(result.Value.Wind.Deg).Within(0.01));
            Assert.That(10000, Is.EqualTo(result.Value.Visibility).Within(0.01));
            Assert.AreEqual("broken clouds", result.Value.SkyConditions);
            Assert.That(tempC, Is.EqualTo(result.Value.TemperatureCelsius).Within(0.01));
            Assert.That(dewPointC, Is.EqualTo(result.Value.DewPointCelsius).Within(0.01));
            Assert.That(humidity, Is.EqualTo(result.Value.RelativeHumidity).Within(0.01));
            Assert.That(1000.0, Is.EqualTo(result.Value.Pressure).Within(0.01));

        }
    }
}