using System;

namespace Limbo.Umbraco.Skyfish.Exceptions;

/// <summary>
/// Class representing a generic Skyfish exception.
/// </summary>
public class SkyfishException : Exception {

    /// <summary>
    /// Initializes a new instance with a generic error message.
    /// </summary>
    public SkyfishException() : base("An error occured on the server.") { }

    /// <summary>
    /// Initializes a new instance with a generic error message and the specified <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    public SkyfishException(Exception? innerException) : base("An error occured on the server.", innerException) { }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public SkyfishException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The inner exception.</param>
    public SkyfishException(string message, Exception? innerException) : base(message, innerException) { }

}