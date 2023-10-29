using System;
using System.Threading.Tasks;

namespace CityWeather.Models
{
    public struct Wind
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }
    
    public struct Weather
    {
        public string Location { get; set; }
        public DateTime Time { get; set; }
        public Wind Wind { get; set; }
        public double Visibility { get; set; }
        public string SkyConditions { get; set; }
        public double TemperatureCelsius { get; set; }
        public double TemperatureFahrenheit { get; set; }
        public double DewPointCelsius { get; set; }
        public double DewPointFahrenheit { get; set; }
        public double RelativeHumidity { get; set; }
        public double Pressure { get; set; }
    }
    
    public interface IWeatherService
    {
        Task<Weather> GetWeather(string cityName);
    }
}