using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityWeather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityWeather.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            this.weatherService = weatherService;
        }

        [HttpGet("{cityName}")]
        public async Task<ActionResult<Weather>> Get(string cityName)
        {
            try
            {
                return await this.weatherService.GetWeather(cityName);
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new { Message = "Invalid city name" });
            }
            catch (Exception)
            {
                return StatusCode(503, new { Message = "Weather service is unavailable" });
            }
        }
    }
}