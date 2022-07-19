using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DemoNothing.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConnectionMultiplexer _redis;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var db = _redis.GetDatabase();
        var redisCachedResult = db.StringGet("myweather");

        if (!redisCachedResult.HasValue)
        {
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });

            var resultSerialized = System.Text.Json.JsonSerializer.Serialize(result);

            db.StringSet("myweather", resultSerialized);
            db.KeyExpire("myweather", DateTime.UtcNow.AddMinutes(3));

            return result;
        }

        return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(redisCachedResult);
    }
}