# Crează o aplicație tip webapi + Azure cache for Redis

## Consolă MacOS

1. Crează un nou webapi (vezi tutorial 02)

2. Crează o instanță de Redis în Azure portal (Type minim = Basic C0, 250MB, 15 Eur/lună)

3. dotnet user-secrets init (declari un fișier, pe discul local, pentru salvarea info senzitive)

4. dotnet user-secrets set CacheConnection "<cache name>.redis.cache.windows.net,abortConnect=false,ssl=true,allowAdmin=true,password=<primary-access-key>" (salvezi connectionString-ul de Redis pe disc);

5. dotnet add package StackExchange.Redis

6. Add file RedisConnection.cs (https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats/RedisConnection.cs)

7. Adaugă în Program.cs:
var redisConnectionString = builder.Configuration["CacheConnection"].ToString();
builder.Services.AddSingleton(async x => await RedisConnection.InitializeAsync(redisConnectionString));

8. Adaugă în Controller:
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


Sursa: https://github.com/Azure-Samples/azure-cache-redis-samples/blob/main/quickstart/aspnet-core/ContosoTeamStats