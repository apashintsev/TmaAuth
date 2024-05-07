using System.Text.Json.Serialization;

namespace TmaAuth;

/// <summary>
/// Represents the information about a Telegram user.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the Telegram user.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the first name of the Telegram user.
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the Telegram user.
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// Gets or sets the username of the Telegram user.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the IETF language tag of the Telegram user's interface language.
    /// </summary>
    [JsonPropertyName("language_code")]
    public string LanguageCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Telegram user has a premium subscription.
    /// </summary>
    [JsonPropertyName("is_premium")]
    public bool IsPremium { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user allows writing to them in private messages.
    /// </summary>
    [JsonPropertyName("allows_write_to_pm")]
    public bool AllowsWriteToPm { get; set; }
}
