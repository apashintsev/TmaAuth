using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace TmaAuth;

/// <summary>
/// A context for <see cref="TmaEvents.OnMessageReceived"/>.
/// </summary>
public class MessageReceivedContext : ResultContext<TelegramAuthenticationOptions>
{
    /// <summary>
    /// Initializes a new instance of <see cref="MessageReceivedContext"/>.
    /// </summary>
    /// <inheritdoc />
    public MessageReceivedContext(
        HttpContext context,
        AuthenticationScheme scheme,
        TelegramAuthenticationOptions options)
        : base(context, scheme, options) { }

    /// <summary>
    ///  TMA Auth. This will give the application an opportunity to retrieve a initDataRaw from an alternative location.
    /// </summary>
    public string? InitDataRaw { get; set; }
}
