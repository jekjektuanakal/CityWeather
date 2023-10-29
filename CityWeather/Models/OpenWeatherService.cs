using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CityWeather.Models
{
    public struct WindResponse
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }

    public struct WeatherDescriptionResponse
    {
        public string Description { get; set; }
    }

    public struct MainResponse
    {
        public double Temp { get; set; }
        public double Pressure { get; set; }
        public double Humidity { get; set; }
    }

    public struct WeatherResponse
    {
        public string Name { get; set; }
        public long Dt { get; set; }
        public WindResponse Wind { get; set; }
        public long Visibility { get; set; }
        public WeatherDescriptionResponse[] Weather { get; set; }
        public MainResponse Main { get; set; }
    }

    public class OpenWeatherService : IWeatherService
    {
        private readonly string apiKey;
        private readonly HttpClient httpClient;

        public OpenWeatherService(HttpClient httpClient, string apiKey)
        {
            this.httpClient = httpClient;
            this.apiKey = apiKey;
        }

        public async Task<Weather> GetWeather(string cityName)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={this.apiKey}&units=metric";
            HttpResponseMessage response = await this.httpClient.GetAsync(url);

            switch (response.StatusCode)
            {
            case HttpStatusCode.OK:
                break;
            case HttpStatusCode.NotFound:
                throw new ArgumentException("City not found");
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new Exception("Invalid API key");
            default:
                throw new Exception($"Error calling OpenWeather API: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            var weatherResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<WeatherResponse>(responseBody);
            
            var dewPointCelsius = CalculateDewPoint(weatherResponse.Main.Temp, weatherResponse.Main.Humidity);

            return new Weather
            {
                Location = weatherResponse.Name,
                Time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(weatherResponse.Dt),
                Wind = new Wind { Speed = weatherResponse.Wind.Speed, Deg = weatherResponse.Wind.Deg },
                Visibility = weatherResponse.Visibility,
                SkyConditions = weatherResponse.Weather[0].Description,
                TemperatureCelsius = weatherResponse.Main.Temp,
                TemperatureFahrenheit = weatherResponse.Main.Temp * 9 / 5 + 32,
                DewPointCelsius = dewPointCelsius,
                DewPointFahrenheit = dewPointCelsius * 9 / 5 + 32,
                RelativeHumidity = weatherResponse.Main.Humidity,
                Pressure = weatherResponse.Main.Pressure
            };
        }
        
        private double CalculateDewPoint(double temperatureCelsius, double relativeHumidity)
        {
            var b = 17.625;
            var c = 243.04;
            
            var gamma = Math.Log(relativeHumidity / 100) + b * temperatureCelsius / (c + temperatureCelsius);
            var dewPoint = c * gamma / (b - gamma);
            
            return dewPoint;
        }
    }
}