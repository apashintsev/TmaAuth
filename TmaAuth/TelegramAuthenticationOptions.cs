namespace TmaAuth;

/// <summary>
/// Specifies options for setting up Telegram authentication.
/// This class provides programmatic configuration for the Telegram authentication handler.
/// </summary>
public class TelegramAuthenticationOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the bot token used for authenticating API requests to the Telegram.
    /// This token is required to interact securely with the Telegram API.
    /// </summary>
    public string BotToken { get; set; }
}
