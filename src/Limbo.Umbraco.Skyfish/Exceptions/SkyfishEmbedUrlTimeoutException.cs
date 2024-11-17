using System;

namespace Limbo.Umbraco.Skyfish.Exceptions;

/// <summary>
/// Exception thrown when an embed code cannnot be created within the allowed time.
/// </summary>
public class SkyfishEmbedUrlTimeoutException : SkyfishException {

    /// <summary>
    /// Gets the source - e.g. the Skyfish app URL of the video.
    /// </summary>
    public new string Source { get; }

    /// <summary>
    /// Gets the unique ID of the media.
    /// </summary>
    public int UniqueMediaId { get; }

    /// <summary>
    /// Gets the maximum amount of attempts.
    /// </summary>
    public int MaxAttempts { get; }

    /// <summary>
    /// Gets the interval between each attempt.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Initializes a new instance base on the specified <paramref name="source"/> and <paramref name="uniqueMediaId"/>.
    /// </summary>
    /// <param name="source">The source - e.g. the Skyfish app URL of the video.</param>
    /// <param name="uniqueMediaId">The unique ID of the media.</param>
    /// <param name="maxAttempts"></param>
    /// <param name="interval"></param>
    public SkyfishEmbedUrlTimeoutException(string source, int uniqueMediaId, int maxAttempts, TimeSpan interval) : base($"Failed getting embed code for media with unique media ID '{uniqueMediaId}'.") {
        Source = source;
        UniqueMediaId = uniqueMediaId;
        MaxAttempts = maxAttempts;
        Interval = interval;
    }

}