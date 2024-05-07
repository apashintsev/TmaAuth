namespace TmaAuth;

/// <summary>
/// Defines the claim types used for Telegram Mini App user authentication.
/// </summary>
public static class TmaClaim
{
    /// <summary>
    /// Claim type for the first name of the user.
    /// This claim stores the first name as provided by the Telegram Mini App user data.
    /// </summary>
    public const string FirstName = "First_Name";

    /// <summary>
    /// Claim type for the last name of the user.
    /// This claim stores the last name as provided by the Telegram Mini App user data.
    /// </summary>
    public const string LastName = "Last_Name";

    /// <summary>
    /// Claim type indicating whether the user has a premium status.
    /// This claim is used to store a boolean value ('true' or 'false') that indicates
    /// if the user is a premium subscriber of the Telegram Mini App.
    /// </summary>
    public const string IsPremium = "Is_Premium";

    /// <summary>
    /// Claim type indicating whether the user allows writing to private messages.
    /// This claim is used to store a boolean value ('true' or 'false') that indicates
    /// whether the user permits sending private messages to them via the Telegram Mini App.
    /// </summary>
    public const string AllowsWriteToPM = "Allows_Write_To_PM";
}
