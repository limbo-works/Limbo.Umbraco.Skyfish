namespace Limbo.Umbraco.Skyfish.Models;

/// <summary>
/// Enum type indicating the type of source for a Skyfish video.
/// </summary>
public enum SkyfishSourceType {

    /// <summary>
    /// Indicates that the source is an app URL.
    /// </summary>
    AppUrl,

    /// <summary>
    /// Indicates that the source is a share URL.
    /// </summary>
    ShareUrl,

    /// <summary>
    /// Indicates that the source is an embed code.
    /// </summary>
    Embed

}