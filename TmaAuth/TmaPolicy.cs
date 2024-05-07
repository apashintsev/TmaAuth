namespace TmaAuth;

/// <summary>
/// Contains constants for authorization policies specific to the Telegram Mini App authentication.
/// </summary>
public static class TmaPolicy
{
    /// <summary>
    /// Authorization policy name for access restricted to premium Telegram Mini App users.
    /// This policy ensures that only premium users can access certain secured resources.
    /// </summary>
    public const string TmaUserPremiumPolicy = "TmaUserPremiumPolicy";
}
