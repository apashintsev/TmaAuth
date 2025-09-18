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
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new TmaEvents Events
    {
        get => (TmaEvents)base.Events!;
        set => base.Events = value;
    }

    /// <summary>
    /// Processes and validates the authentication request.
    /// </summary>
    /// <returns>The result of the authentication process.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? initDataRaw;

        try
        {
            // Give application opportunity to find from a different location, adjust, or reject token
            var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

            // event can set the token
            await Events.MessageReceived(messageReceivedContext);
            if (messageReceivedContext.Result != null)
            {
                return messageReceivedContext.Result;
            }

            // If application retrieved token from somewhere else, use that.
            initDataRaw = messageReceivedContext.InitDataRaw;

            if (string.IsNullOrEmpty(initDataRaw))
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authorizationHeader) &&
                    authorizationHeader.StartsWith("tma ", StringComparison.OrdinalIgnoreCase))
                {
                    initDataRaw = authorizationHeader.Substring(4);
                }
            }

            if (string.IsNullOrEmpty(initDataRaw))
                return AuthenticateResult.NoResult();

            ValidateInitData(initDataRaw);

            var parsedData = HttpUtility.ParseQueryString(initDataRaw);
            var userDataJson = parsedData["user"];
            var user = JsonSerializer.Deserialize<UserInfo>(userDataJson);

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Username)?"Unknown":user.Username),
                new (TmaClaim.FirstName, string.IsNullOrWhiteSpace(user.FirstName)?"Unknown":user.FirstName),
                new (TmaClaim.LastName, string.IsNullOrWhiteSpace(user.LastName) ? "Unknown" : user.LastName),
                new (ClaimTypes.Locality, user.LanguageCode),
                new (TmaClaim.IsPremium, user.IsPremium.ToString()),
                //new (TmaClaim.AllowsWriteToPM, user.AllowsWriteToPm.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (TelegramValidationException ex)
        {
            return AuthenticateResult.Fail(ex.Message);
        }
    }

    private bool ValidateInitData(string initData, int expiresIn = 86400)
    {
        var searchParams = HttpUtility.ParseQueryString(initData);

        // Retrieve the hash and remove it from the collection for data validation
        var hash = searchParams["hash"];
        if (string.IsNullOrEmpty(hash))
            throw new TelegramValidationException("Hash is empty or not found.");
        searchParams.Remove("hash");

        // Validate and parse the 'auth_date'
        if (!searchParams.AllKeys.Contains("auth_date") || !long.TryParse(searchParams["auth_date"], out long authDateUnix))
            throw new TelegramValidationException("'auth_date' should present integer or is missing.");

        var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix);
        if (authDate == DateTimeOffset.FromUnixTimeSeconds(0))
            throw new TelegramValidationException("'auth_date' is empty or not found.");

        // Check if the data has expired
        if (expiresIn > 0 && DateTimeOffset.UtcNow > authDate.AddSeconds(expiresIn))
            throw new TelegramValidationException("Init data expired.");

        // Prepare data for signature validation
        var pairs = searchParams.AllKeys
                                .OrderBy(key => key)
                                .Select(key => $"{key}={searchParams[key]}")
                                .ToList();

        var dataToValidate = string.Join("\n", pairs);

        // Compute HMAC using 'WebAppData' and the bot token
        var intermediateKeyBytes = ComputeHMACSHA256Hash(Encoding.UTF8.GetBytes("WebAppData"), Options.BotToken);
        var computedHash = ComputeHMACSHA256Hex(dataToValidate, intermediateKeyBytes);

        // Verify the computed hash with the provided hash
        if (!computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase))
            throw new TelegramValidationException("Signature is invalid");
        return true;
    }

    private static byte[] ComputeHMACSHA256Hash(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string ComputeHMACSHA256Hex(string data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
    }
}
