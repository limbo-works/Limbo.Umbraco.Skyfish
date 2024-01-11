using System;
using HtmlAgilityPack;
using Limbo.Umbraco.Skyfish.PropertyEditors;
using Limbo.Umbraco.Video.Models.Videos;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Http.Collections;
using Skybrud.Essentials.Json.Newtonsoft.Converters;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;

namespace Limbo.Umbraco.Skyfish.Models.Videos;

/// <summary>
/// Class with embed information for a Skyfish video.
/// </summary>
public class SkyfishVideoEmbed : IVideoEmbed {

    private readonly SkyfishVideoDetails _details;

    #region Properties

    /// <summary>
    /// Gets the embed URL.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; }

    /// <summary>
    /// Indicates whether the video should automatically start to play when the player loads.
    /// </summary>
    [JsonIgnore]
    protected bool Autoplay { get; }

    /// <summary>
    /// Gets the HTML embed code.
    /// </summary>
    [JsonProperty("html")]
    [JsonConverter(typeof(StringJsonConverter))]
    public IHtmlContent Html { get; }

    #endregion

    #region Constructors

    internal SkyfishVideoEmbed(JObject json, SkyfishVideoDetails details, SkyfishConfiguration? configuration) {
        _details = details ?? throw new ArgumentNullException(nameof(details));
        Url = json.GetString("url")!;
        Autoplay = false;
        Html = GetHtml();
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

        iframe.Attributes.Add("src", Url);

        var allowString = "accelerometer; encrypted-media; gyroscope; picture-in-picture";

        if (Autoplay) allowString += "; autoplay";

        iframe.Attributes.Add("allow", allowString);

        iframe.Attributes.Add("allowfullscreen", string.Empty);
        iframe.Attributes.Add("webkitallowfullscreen", string.Empty);
        iframe.Attributes.Add("mozallowfullscreen", string.Empty);

        // All iFrame attributes are taken from https://player.skyfish.com/iframe.json
        iframe.Attributes.Add("onLoad", "!function(e){try{e.style.maxWidth='100%';var n=e.width/e.height;if(0<n){var t,d;(t=function(){var t,i=e.getBoundingClientRect();t=i.width<e.width?(i.width/n).toFixed(0)+'px':'none',d!==t&&(e.style.maxHeight=d=t)})(),window.addEventListener('resize',t)}}catch(t){console.log(t)}}(this)");

        if (string.IsNullOrWhiteSpace(_details.Title) == false) iframe.Attributes.Add("title", _details.Title);

        return new HtmlString(
            iframe.OuterHtml
                .Replace("mozallowfullscreen=\"\"", "mozallowfullscreen")
                .Replace("webkitallowfullscreen=\"\"", "webkitallowfullscreen")
                .Replace("allowfullscreen=\"\"", "allowfullscreen")
        );
    }

    #endregion

}