using HtmlAgilityPack;
using Limbo.Umbraco.Skyfish.Models.Videos;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Skybrud.Essentials.Http.Collections;

namespace Limbo.Umbraco.Skyfish.Options;

/// <summary>
/// Class representing the embed options for a Skyfish video
/// </summary>
public class SkyfishEmbedOptions {

    #region Properties

    /// <summary>
    /// Gets the ID of the video.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; }

    /// <summary>
    /// Gets the embed URL.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; }

    /// <summary>
    /// Gets the title of the video, if any.
    /// </summary>
    [JsonIgnore]
    public string? Title { get; }

    /// <summary>
    /// Gets whether the embed code requires prior consent before being show to the user.
    /// </summary>
    [JsonProperty("consent")]
    public bool RequireConsent { get; }

    /// <summary>
    /// Indicates whether the video should automatically start to play when the player loads.
    /// </summary>
    [JsonProperty("autoplay")]
    public bool Autoplay { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="id"/> and <paramref name="embedUrl"/>.
    /// </summary>
    /// <param name="id">The ID of the media.</param>
    /// <param name="embedUrl">The embed URL of the video.</param>
    public SkyfishEmbedOptions(int id, string embedUrl) {
        Id = id;
        Url = embedUrl;
    }

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="details"/> and <paramref name="embedUrl"/>.
    /// </summary>
    /// <param name="details">The details describing the video.</param>
    /// <param name="embedUrl">The embed URL of the video.</param>
    public SkyfishEmbedOptions(SkyfishVideoDetails details, string embedUrl) {
        Id = details.Id;
        Url = embedUrl;
        Title = details.Title;
    }

    #endregion

    #region Member methods

    /// <summary>
    /// Returns the HTML embed code for an iframe with a width of 854 pixels and a height of 480 pixels.
    /// </summary>
    /// <returns>The HTML embed code for the Skyfish video.</returns>
    public IHtmlContent GetHtml() {
        return GetHtml(854, 480);
    }

    /// <summary>
    /// Returns the HTML embed code for an iframe with the specified <paramref name="width"/> and <paramref name="height"/>.
    /// </summary>
    /// <param name="width">The width of the iframe.</param>
    /// <param name="height">The height of the iframe.</param>
    /// <returns>The HTML embed code for the Skyfish video.</returns>
    public IHtmlContent GetHtml(int width, int height) {

        HttpQueryString query = new();
        if (Autoplay) query.Add("autoplay", "true");

        HtmlDocument document = new();

        HtmlNode iframe = document.CreateElement("iframe");

        iframe.Attributes.Add("frameborder", "0");
        iframe.Attributes.Add("width", width.ToString());
        iframe.Attributes.Add("height", height.ToString());

        iframe.Attributes.Add(RequireConsent ? "consent-src" : "src", Url);

        string allowString = "accelerometer; encrypted-media; gyroscope; picture-in-picture";

        if (Autoplay) allowString += "; autoplay";

        iframe.Attributes.Add("allow", allowString);

        iframe.Attributes.Add("allowfullscreen", string.Empty);
        iframe.Attributes.Add("webkitallowfullscreen", string.Empty);
        iframe.Attributes.Add("mozallowfullscreen", string.Empty);

        // All iFrame attributes are taken from https://player.skyfish.com/iframe.json
        iframe.Attributes.Add("onLoad", "!function(e){try{e.style.maxWidth='100%';var n=e.width/e.height;if(0<n){var t,d;(t=function(){var t,i=e.getBoundingClientRect();t=i.width<e.width?(i.width/n).toFixed(0)+'px':'none',d!==t&&(e.style.maxHeight=d=t)})(),window.addEventListener('resize',t)}}catch(t){console.log(t)}}(this)");

        if (string.IsNullOrWhiteSpace(Title) == false) iframe.Attributes.Add("title", Title);

        return new HtmlString(
            iframe.OuterHtml
                .Replace("mozallowfullscreen=\"\"", "mozallowfullscreen")
                .Replace("webkitallowfullscreen=\"\"", "webkitallowfullscreen")
                .Replace("allowfullscreen=\"\"", "allowfullscreen")
        );

    }

    #endregion

}