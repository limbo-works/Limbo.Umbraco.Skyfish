using System;
using System.Collections.Generic;
using Limbo.Umbraco.Video.Models.Videos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Converters.Time;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Json.Newtonsoft;

namespace Limbo.Umbraco.Skyfish.Models.Videos;

/// <summary>
/// Class with details about a Skyfish video.
/// </summary>
public class SkyfishVideoDetails : IVideoDetails {

    #region Properties

    /// <summary>
    /// Gets the unique media ID of the video.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; }

    /// <summary>
    /// Gets the title of the video, if any.
    /// </summary>
    [JsonProperty("title")]
    public string? Title { get; }

    string IVideoDetails.Title => Title ?? string.Empty;

    /// <summary>
    /// Gets the description of the video, if any.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; }

    /// <summary>
    /// Gets the file name of the video, if any.
    /// </summary>
    [JsonProperty("fileName")]
    public string? FileName { get; }

    /// <summary>
    /// Gets the width of the video.
    /// </summary>
    [JsonProperty("width")]
    public int Width { get; }

    /// <summary>
    /// Gets the height of the video.
    /// </summary>
    [JsonProperty("height")]
    public int Height { get; }

    /// <summary>
    /// Gets the duration of the video.
    /// </summary>
    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(TimeSpanSecondsConverter))]
    public TimeSpan? Duration { get; }

    /// <summary>
    /// Gets a list of thumbnails of the video.
    /// </summary>
    [JsonProperty("thumbnails")]
    public IEnumerable<IVideoThumbnail> Thumbnails { get; }

    /// <summary>
    /// Gets an array with the files of the video. This will currently always be empty.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<IVideoFile> Files { get; }

    #endregion

    #region Constructors

    private SkyfishVideoDetails(JObject json) {

        JObject data = json.GetString("_data", JsonUtils.ParseJsonObject)!;

        Id = data.GetInt32("uniqueMediaId");
        Title = data.GetString("title");
        Description = data.GetString("description");
        FileName = data.GetString("fileName");
        Width = data.GetInt32("width");
        Height = data.GetInt32("height");
        Duration = data.GetDouble("duration", TimeSpan.FromSeconds);
        Thumbnails = data.GetArrayItems("thumbnails", x => (IVideoThumbnail) VideoThumbnail.Parse(x)!);
        Files = Array.Empty<IVideoFile>();

    }

    #endregion

    #region Static methods

    /// <summary>
    /// Returns a new <see cref="SkyfishVideoDetails"/> parsed from the specified <paramref name="json"/>.
    /// </summary>
    /// <param name="json">The instance of <see cref="JObject"/> to parse.</param>
    /// <returns>An instance of <see cref="SkyfishVideoDetails"/>.</returns>
    public static SkyfishVideoDetails? Parse(JObject? json) {
        return json == null ? null : new SkyfishVideoDetails(json);
    }

    #endregion

}