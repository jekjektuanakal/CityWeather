using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CityWeather.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace CityWeather.Test
{
    public class OpenWeatherHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var city = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("q");
            
            return Task.FromResult(
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(
                        @"{
                        ""name"": """ + city + @""",
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
                            ""temp"": 30,
                            ""pressure"": 1000,
                            ""humidity"": 100
                        }
                    }"),
                });
        }
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(
                services =>
                {
                    var httpClient = new HttpClient(new OpenWeatherHttpMessageHandler());
                    services.AddSingleton<IWeatherService>(new OpenWeatherService(httpClient, "dummy"));
                });

            return base.CreateHost(builder);
        }
    }

    [TestFixture]
    public class ServiceTests
    {
        private TestWebApplicationFactory factory;
        private HttpClient client;

        [SetUp]
        public void SetUp()
        {
            this.factory = new TestWebApplicationFactory();
            
            var opts = new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
            };
            this.client = this.factory.CreateClient(opts);
        }

        [TearDown]
        public void TearDown()
        {
            this.factory.Dispose();
        }

        [Test]
        public async Task GetCountries_ReturnCountries()
        {
            HttpResponseMessage result = await this.client.GetAsync("/countries");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);

            var countriesJson = await result.Content.ReadAsStringAsync();
            var countries = Newtonsoft.Json.JsonConvert.DeserializeObject<Country[]>(countriesJson);

            Assert.That(countries, Contains.Item(new Country { Name = "Indonesia", Code = "ID" }));
            Assert.That(countries, Contains.Item(new Country { Name = "Singapore", Code = "SG" }));
            Assert.That(countries, Contains.Item(new Country { Name = "Australia", Code = "AU" }));
        }
        
        [Test]
        public async Task GetCities_InvalidCountry_ReturnBadRequest()
        {
            HttpResponseMessage result = await this.client.GetAsync("/countries/SG/cities");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);
            
            var citiesJson = await result.Content.ReadAsStringAsync();
            var cities = Newtonsoft.Json.JsonConvert.DeserializeObject<City[]>(citiesJson);
            
            Assert.That(cities, Contains.Item(new City { Name = "Changi", Country = "SG", Location = "CHG" }));
        }

        [Test]
        public async Task GetWeather_ValidCity_ReturnWeather()
        {
            HttpResponseMessage result = await this.client.GetAsync("/weather/Yogyakarta");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCode);

            var weatherJson = await result.Content.ReadAsStringAsync();
            var weather = Newtonsoft.Json.JsonConvert.DeserializeObject<Weather>(weatherJson);

            Assert.AreEqual("Yogyakarta", weather.Location);
            Assert.AreEqual("broken clouds", weather.SkyConditions);
            Assert.AreEqual(10.0, weather.Wind.Speed);
            Assert.AreEqual(90.0, weather.Wind.Deg);
        }
    }
}