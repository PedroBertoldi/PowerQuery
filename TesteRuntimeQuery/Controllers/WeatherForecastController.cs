using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TesteRuntimeQuery.Data;
using TesteRuntimeQuery.Models;
using CustomGrouping;
using PowerQueryV2;

namespace TesteRuntimeQuery.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DatabaseContext _databaseContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DatabaseContext databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("GetText")]
        public IEnumerable<TestModel> GetText([FromQuery] string param)
        {
            return _databaseContext.TestModel
                .Include(x => x.TestChildModels)
                .QueryV2(param)
                .ToList();
        }

        [HttpGet("grouping")]
        public IActionResult GroupTestings()
        {
            var data = _databaseContext.GroupTestingClass
                .GroupByClass(nameof(GroupTestingClass.Id))
                .Select(x => new { count = x.Count(), x.Key })
                .ToList();

            return Ok(data);
        }
    }
}
