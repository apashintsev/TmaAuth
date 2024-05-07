namespace TmaAuth;


/// <summary>
/// Represents errors that occur during the validation of Telegram authentication data.
/// This custom exception is thrown when specific validation checks fail,
/// such as signature mismatch, missing required fields, or data expiration.
/// </summary>
public class TelegramValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TelegramValidationException(string message) : base(message) { }
}