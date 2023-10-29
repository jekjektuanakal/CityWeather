using System.Collections.Generic;
using System.Threading.Tasks;

namespace CityWeather.Models
{
    public struct Country
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    
    public struct City
    {
        public string Country { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }
    
    public interface ICountryService
    {
        Task<List<Country>> GetCountries();
        Task<List<City>> GetCities(string alpha2CountryCode);
    }
}