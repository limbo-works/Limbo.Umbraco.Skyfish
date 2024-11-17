namespace Limbo.Umbraco.Skyfish.Exceptions;

/// <summary>
/// Exception class thrown when a requested video is not found.
/// </summary>
public class SkyfishVideoNotFoundException : SkyfishException {

    /// <summary>
    /// Gets the source value.
    /// </summary>
    public new string Source { get; }

    /// <summary>
    /// Initialize a new instance based on the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source value - either a video URL or embed code.</param>
    public SkyfishVideoNotFoundException(string source) : base("A video with the specified URL or embed code could not be found.") {
        Source = source;
    }

}