using Microsoft.AspNetCore.Mvc;

namespace _03_net6_webapi_azure_redis.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Task<RedisConnection> _redisConnectionFactory;
    private RedisConnection _redisConnection;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, Task<RedisConnection> redisConnectionFactory)
    {
        _logger = logger;
        _redisConnectionFactory = redisConnectionFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {
        _redisConnection = await _redisConnectionFactory;

        // Simple PING command
        var command1 = "PING";
        var command1Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync(command1))).ToString();

        // Simple get and set into the cache
        string key = "Message";
        string value = "Hello! The cache is working from ASP.NET Core!";

        var command2 = $"SET {key} \"{value}\"";
        var command2Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(key, value))).ToString();

        var command3 = $"GET {key}";
        var command3Result = (await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(key))).ToString();

        var isSuccess = command1Result == "PONG" && command3Result == value;
        return Ok($"Is success: {isSuccess}");
    }
}
