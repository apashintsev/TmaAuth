using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using System.Web;
using System.Text.Json;

namespace TmaAuth;

/// <summary>
/// Handles authentication for Telegram Mini App based on the custom scheme and initialization data.
/// </summary>
public class TmaAuthenticationHandler : AuthenticationHandler<TelegramAuthenticationOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="TmaAuthenticationHandler"/>.
    /// </summary>
    /// <param name="options">Monitored options for Telegram authentication settings.</param>
    /// <param name="logger">Factory to create a logger.</param>
    /// <param name="encoder">Encoder for the URLs.</param>
    /// <param name="clock">System clock for timing operations.</param>
    public TmaAuthenticationHandler(
        IOptionsMonitor<TelegramAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    /// <summary>
    /// Processes and validates the authentication request.
    /// </summary>
    /// <returns>The result of the authentication process.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("tma "))
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }

        string initDataRaw = authorizationHeader.Substring(4);
        bool isValid = ValidateInitData(initDataRaw);
        if (!isValid)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        var parsedData = HttpUtility.ParseQueryString(initDataRaw);
        var userDataJson = parsedData["user"];
        var user = JsonSerializer.Deserialize<UserInfo>(userDataJson);

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Name, user.Username),
            new (TmaClaim.FirstName, user.FirstName),
            new (TmaClaim.LastName, user.LastName),
            new (ClaimTypes.Locality, user.LanguageCode),
            new (TmaClaim.IsPremium, user.IsPremium.ToString()),
            new (TmaClaim.AllowsWriteToPM, user.AllowsWriteToPm.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Validates the initialization data from Telegram against a computed hash to ensure authenticity.
    /// </summary>
    /// <param name="initData">Raw initialization data containing user information and a hash.</param>
    /// <returns><c>true</c> if the validation is successful; otherwise, <c>false</c>.</returns>
    private bool ValidateInitData(string initData)
    {
        var parts = initData.Split('&').OrderBy(s => s).ToArray();
        var hash = parts.FirstOrDefault(p => p.StartsWith("hash="))?.Substring(5);
        if (hash == null)
        {
            return false;
        }

        var dataToValidate = string.Join("\n", parts.Where(p => !p.StartsWith("hash=")));

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData")))
        {
            var hashKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(Options.BotToken));

            using (var hmacData = new HMACSHA256(hashKey))
            {
                var computedHash = hmacData.ComputeHash(Encoding.UTF8.GetBytes(dataToValidate));
                var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLowerInvariant();

                return hash.Equals(computedHashString, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
