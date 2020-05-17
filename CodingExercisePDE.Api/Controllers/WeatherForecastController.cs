using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodingExercisePDE.Entities;
using CodingExercisePDE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodingExercisePDE.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IRepository<RandomNumber> _repository;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController
            (
            IRepository<RandomNumber> repository,
            ILogger<WeatherForecastController> logger)

        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {                    

            var rng = new Random();
            var all = await _repository.GetAllAsync();

            int x = 1;
            

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
