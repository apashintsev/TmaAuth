using Microsoft.AspNetCore.Authentication;

namespace TmaAuth;

/// <summary>
/// Extension methods to configure Telegram Mini App authentication.
/// </summary>
public static class TelegramMiniAppAuthenticationExtensions
{
    /// <summary>
    /// Adds Telegram Mini App authentication to the specified <see cref="AuthenticationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the authentication to.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="Telegram Mini AppAuthenticationOptions"/>.</param>
    /// <returns>The updated <see cref="AuthenticationBuilder"/>.</returns>
    /// <remarks>
    /// This extension method sets up Telegram Mini App authentication with the specified options configuration.
    /// The method is used to integrate Telegram Mini App as an authentication provider in the application's authentication system.
    /// Use this method in the <c>ConfigureServices</c> method of your <c>Startup.cs</c> file to configure the authentication handler
    /// that checks for Telegram Mini App credentials in requests.
    /// </remarks>
    public static AuthenticationBuilder AddTelegramMiniAppToken(this AuthenticationBuilder builder, Action<TelegramAuthenticationOptions> configureOptions)
    {
        return builder.AddScheme<TelegramAuthenticationOptions, TmaAuthenticationHandler>(
            $"{TmaTokenDefaults.AuthenticationScheme}",
            configureOptions);
    }
}
