using Microsoft.Extensions.Caching.Distributed;
using Shonos.FluentCache;
using Shonos.FluentCache.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Setup to use IDistributedCache
// Configure to use in-memory distributed cache
builder.Services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
builder.Services.AddTransient<IFluentCache, FluentCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IFluentCache cache, ILogger<Program> logger) =>
{
    // Create a key for caching
    const string cacheKey = "weatherforecast";

    // Use CreateWithOnCacheMiss to handle cache miss
    var forecast = await cache.CreateWithOnCacheMiss(() =>
    {
        logger.LogInformation("Cache missed!");
        var result = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return Task.FromResult(result);
    })
    .CacheIf(x => x != null)
    .SetAbsoluteExpirationRelativeToNow(TimeSpan.FromHours(12))
    .GetAsync(cacheKey)
    .ConfigureAwait(false);

    // Return the result as JSON
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
