using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TmaAuth;

namespace Example.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = TmaTokenDefaults.AuthenticationScheme)]
    public IActionResult GetSecureData()
    {
        // Получение идентификатора пользователя из claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok($"Secure data accessed by user ID {userId} with username {username}");
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = TmaTokenDefaults.AuthenticationScheme, Policy = TmaPolicy.TmaUserPremiumPolicy)]
    public IActionResult GetForPremium()
    {
        return Ok("This is data only for premium users.");
    }
}
