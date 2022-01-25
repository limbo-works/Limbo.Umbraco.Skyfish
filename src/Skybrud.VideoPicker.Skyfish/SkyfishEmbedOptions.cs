using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Skybrud.Essentials.Http.Collections;
using Skybrud.VideoPicker.Models.Videos;

namespace Skybrud.VideoPicker.Skyfish {

    public class SkyfishEmbedOptions : IVideoEmbedOptions {

        private readonly SkyfishVideoDetails _details;

        #region Properties

        [JsonProperty("url")]
        public string Url { get; }

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

        public SkyfishEmbedOptions(SkyfishVideoDetails details) : this(details, null) { }

        public SkyfishEmbedOptions(SkyfishVideoDetails details, SkyfishDataTypeConfig config) {

            _details = details;
            Url = details.EmbedUrl;
            config = config ?? new SkyfishDataTypeConfig();

            RequireConsent = config.RequireConsent.Value;
            Autoplay = config.Autoplay.Value;

        }

        #endregion

        #region Member methods

        public IHtmlString GetHtml() {
            return GetHtml(854, 480);
        }

        public IHtmlString GetHtml(int width, int height) {

            HttpQueryString query = new HttpQueryString();
            if (Autoplay) query.Add("autoplay", "true");

            string embedUrl = _details.EmbedUrl;

            HtmlDocument document = new HtmlDocument();

            HtmlNode iframe = document.CreateElement("iframe");

            iframe.Attributes.Add("frameborder", "0");
            iframe.Attributes.Add("width", width.ToString());
            iframe.Attributes.Add("height", height.ToString());

            iframe.Attributes.Add(RequireConsent ? "consent-src" : "src", embedUrl);

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

}