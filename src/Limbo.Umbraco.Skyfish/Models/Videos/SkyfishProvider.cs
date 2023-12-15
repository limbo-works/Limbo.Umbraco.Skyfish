using Limbo.Umbraco.Video.Models.Providers;

namespace Limbo.Umbraco.Skyfish.Models.Videos;

/// <summary>
/// Class with limited information about a video provider.
/// </summary>
public class SkyfishProvider : VideoProvider {

    /// <summary>
    /// Gets a reference to a <see cref="SkyfishProvider"/> instance.
    /// </summary>
    public static readonly SkyfishProvider Default = new();

    private SkyfishProvider() : base("skyfish", "Skyfish") { }

}