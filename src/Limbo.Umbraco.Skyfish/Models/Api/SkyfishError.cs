using Newtonsoft.Json;

namespace Limbo.Umbraco.Skyfish.Models.Api;

/// <summary>
/// Class representing an error to be presented to the user.
/// </summary>
public class SkyfishError {

    /// <summary>
    /// Gets the error message.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="message"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public SkyfishError(string message) {
        Message = message;
    }

}