using Limbo.Umbraco.Skyfish.Models;

namespace Limbo.Umbraco.Skyfish.Options;

/// <summary>
/// Class with options describing a video.
/// </summary>
public class SkyfishVideoOptions {

    /// <summary>
    /// Gets the raw source (e.g. the app URL, share URL or embed code).
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// gets the type of the source (e.g. <see cref="SkyfishSourceType.AppUrl"/> or <see cref="SkyfishSourceType.Embed"/>).
    /// </summary>

    public SkyfishSourceType Type { get; }

    /// <summary>
    /// Gets the ID of the media.
    /// </summary>
    public int? MediaId { get; }

    /// <summary>
    /// Gets the unique media ID of the media.
    /// </summary>
    public int? UniqueMediaId { get; }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="mediaId"/> and <paramref name="uniqueMediaId"/>.
    /// </summary>
    /// <param name="source">The raw source.</param>
    /// <param name="type">The type of the source.</param>
    /// <param name="mediaId">The ID of the media.</param>
    /// <param name="uniqueMediaId">The unique ID of the media.</param>
    public SkyfishVideoOptions(string source, SkyfishSourceType type, int? mediaId, int? uniqueMediaId) {
        Source = source;
        Type = type;
        MediaId = mediaId;
        UniqueMediaId = uniqueMediaId;
    }

}