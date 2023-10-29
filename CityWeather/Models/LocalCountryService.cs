using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityWeather.Models
{
    public class LocalCountryService : ICountryService
    {
        private readonly IReadOnlyCollection<Country> countries;
        private readonly IReadOnlyCollection<City> cities;

        public LocalCountryService(IReadOnlyCollection<Country> countries, IReadOnlyCollection<City> cities)
        {
            if (countries == null || countries.Count == 0)
            {
                throw new System.ArgumentException("countries cannot be null or empty");
            }
            
            if (cities == null || cities.Count == 0)
            {
                throw new System.ArgumentException("cities cannot be null or empty");
            }
            
            this.countries = countries;
            this.cities = cities;
        }
        
        public Task<List<Country>> GetCountries()
        {
            return Task.FromResult(this.countries.ToList());
        }

        public Task<List<City>> GetCities(string alpha2CountryCode)
        {
            var citiesByCountry = this.cities.Where(city => city.Country == alpha2CountryCode).ToList();
            
            if (citiesByCountry.Count == 0)
            {
                throw new System.ArgumentException("Invalid country code");
            }
            
            return Task.FromResult(citiesByCountry);
        }
    }
}