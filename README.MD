# Telegram Mini App Authentication Extensions for ASP.NET Core

This library provides an easy-to-use extension for integrating Telegram Mini App authentication into your ASP.NET Core applications. It leverages custom authentication schemes to secure your applications using Telegram's user data.

## Features

- Easy integration with ASP.NET Core's authentication system.
- Supports customization of Telegram authentication settings through `TelegramAuthenticationOptions`.
- Utilizes claims-based identity to enhance application security and user management.

## Getting Started

### Installation

To install the Telegram Authentication Extensions, you can use the following NuGet command:

```bash
dotnet add package tmaauth --version 1.0.0
```

## Usage

To secure an API endpoint using the Telegram Mini App authentication scheme, simply use the `Authorize` attribute:

```csharp
            builder.Services.AddAuthentication()
            .AddTelegramMiniAppToken(options =>
            {
                options.BotToken = builder.Configuration["TelegramOptions:BotToken"];
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(TmaPolicy.TmaUserPremiumPolicy, policy =>
                {
                    policy.AddAuthenticationSchemes(TmaTokenDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                    policy.RequireClaim(TmaClaim.IsPremium, "true");  // Требуем, чтобы пользователь был премиум-пользователем
                });
            });
```


```csharp
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
```