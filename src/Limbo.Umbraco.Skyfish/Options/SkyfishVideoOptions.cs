namespace Limbo.Umbraco.Skyfish.Options;

/// <summary>
/// Class with options describing a video.
/// </summary>
public class SkyfishVideoOptions {

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
    /// <param name="mediaId">The ID of the media.</param>
    /// <param name="uniqueMediaId">The unique ID of the media.</param>
    public SkyfishVideoOptions(int? mediaId, int? uniqueMediaId) {
        MediaId = mediaId;
        UniqueMediaId = uniqueMediaId;
    }

}