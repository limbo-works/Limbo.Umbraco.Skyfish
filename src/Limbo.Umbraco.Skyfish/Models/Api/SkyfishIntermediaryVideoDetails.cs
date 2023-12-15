using System;
using System.Collections.Generic;
using Limbo.Integrations.Skyfish.Models.Media;
using Limbo.Umbraco.Video.Models.Videos;
using Newtonsoft.Json;
using Skybrud.Essentials.Json.Converters.Time;

namespace Limbo.Umbraco.Skyfish.Models.Api;

internal class SkyfishIntermediaryVideoDetails {

    [JsonProperty("mediaId")]
    public int MediaId { get; }

    [JsonProperty("uniqueMediaId")]
    public int UniqueMediaId { get; }

    /// <summary>
    /// Gets the title of the video.
    /// </summary>
    [JsonProperty("title")]
    public string? Title { get; }

    /// <summary>
    /// Gets the description of the video.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; }

    [JsonProperty("mimeType")]
    public string MimeType { get; }

    [JsonProperty("fileName")]
    public string FileName { get; }

    [JsonProperty("width")]
    public int Width { get; }

    [JsonProperty("height")]
    public int Height { get; }

    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(TimeSpanSecondsConverter))]
    public TimeSpan? Duration { get; }

    [JsonProperty("folderId")]
    public int FolderId { get; }

    [JsonProperty("thumbnails")]
    public IReadOnlyList<IVideoThumbnail> Thumbnails { get; }

    public SkyfishIntermediaryVideoDetails(SkyfishMediaItem media, TimeSpan? duration, IReadOnlyList<IVideoThumbnail> thumbnails) {
        MediaId = media.MediaId;
        UniqueMediaId = media.UniqueMediaId;
        Title = media.Title;
        Description = media.Description;
        MimeType = media.FileMimeType!;
        FileName = media.FileName!;
        Width = media.Width;
        Height = media.Height;
        Duration = duration;
        FolderId = media.FolderId;
        Thumbnails = thumbnails;
    }

}