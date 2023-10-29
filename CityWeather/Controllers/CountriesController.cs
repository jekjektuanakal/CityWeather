using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityWeather.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityWeather.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService countryService;

        public CountriesController(ICountryService countryService)
        {
            this.countryService = countryService;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<Country>> >Get()
        {
            try {
                return await this.countryService.GetCountries();
            } catch {
                return StatusCode(503, new { Message = "Country service is unavailable" });
            }
        }
        
        [HttpGet("{alpha2CountryCode}/cities")]
        public async Task<ActionResult<List<City>>> Get(string alpha2CountryCode)
        {
            try
            {
                return await this.countryService.GetCities(alpha2CountryCode);
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { Message = "Invalid country code" });
            }
            catch
            {
                return StatusCode(503, new { Message = "Country service is unavailable" });
            }
        }
    }
}