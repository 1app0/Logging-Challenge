using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.logging.Extensions;
using app.logging.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSerilog(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddTransient<LogContextProvider>();
builder.Services.AddSingleton<IWeatherService, FakeWeatherService>();

var app = builder.Build();

app.UseLogContextProviderMiddleware();

app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
});

app.UseExceptionHandler();

app.UseStatusCodePages();


app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello world called");

    return "HELLO WORLD!";
});

app.MapGet("/weather/current", async (string city, IWeatherService weatherService, ILogger<Program> logger) =>
{
    var result = await weatherService.GetCurrentWeatherAsync(city);
    logger.LogInformation("Successfully fetched weather data for {City}", city);
    return Results.Ok(result);
});

app.MapGet("/weather/forecast", async (string city, IWeatherService weatherService, ILogger<Program> logger) =>
{
    var result = await weatherService.GetForecastAsync(city);
    logger.LogInformation("Successfully fetched forecast data for {City}", city);
    return Results.Ok(result);
});

app.Run();

public record WeatherInfo(string City, string Condition, double TemperatureCelsius);

public interface IWeatherService
{
    Task<WeatherInfo> GetCurrentWeatherAsync(string city);
    Task<IEnumerable<WeatherInfo>> GetForecastAsync(string city);
}

public class FakeWeatherService : IWeatherService
{
    private static readonly string[] Conditions =
        { "Sunny", "Cloudy", "Rainy", "Windy", "Snowy" };

    private readonly Random _random = new();

    public Task<WeatherInfo> GetCurrentWeatherAsync(string city)
    {
        if (_random.Next(0, 10) < 2) throw new Exception("External weather provider failed.");

        var weather = new WeatherInfo(
            city,
            Conditions[_random.Next(Conditions.Length)],
            _random.Next(-10, 35)
        );

        return Task.FromResult(weather);
    }

    public Task<IEnumerable<WeatherInfo>> GetForecastAsync(string city)
    {
        var forecast = Enumerable.Range(1, 5).Select(_ => new WeatherInfo(
            city,
            Conditions[_random.Next(Conditions.Length)],
            _random.Next(-10, 35)
        ));

        return Task.FromResult(forecast);
    }
}