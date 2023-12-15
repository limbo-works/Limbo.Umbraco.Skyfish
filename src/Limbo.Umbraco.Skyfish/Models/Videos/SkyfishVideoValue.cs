using Limbo.Umbraco.Skyfish.PropertyEditors;
using Limbo.Umbraco.Video.Models.Providers;
using Limbo.Umbraco.Video.Models.Videos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;

namespace Limbo.Umbraco.Skyfish.Models.Videos {

    /// <summary>
    /// Class representing the value returned by the <see cref="SkyfishEditor"/> property editor.
    /// </summary>
    public class SkyfishVideoValue : IVideoValue {

        #region Properties

        /// <summary>
        /// Gets a reference to the underlying JSON the instance was parsed from.
        /// </summary>
        [JsonIgnore]
        public JObject Json { get; }

        /// <summary>
        /// Gets the source (URL or embed code) as entered by the user.
        /// </summary>
        [JsonIgnore]
        public string? Source { get; }

        /// <summary>
        /// Gets information about the video provider.
        /// </summary>
        [JsonProperty("provider")]
        public SkyfishProvider Provider { get; }

        /// <summary>
        /// Gets the type of the video.
        /// </summary>
        [JsonProperty("type", Order = -99)]
        public string Type { get; }

        /// <summary>
        /// Gets a reference to the video details.
        /// </summary>
        [JsonProperty("details")]
        public SkyfishVideoDetails Details { get; }

        /// <summary>
        /// Gets a reference to the video embed information.
        /// </summary>
        [JsonProperty("embed")]
        public SkyfishVideoEmbed Embed { get; }

        IVideoProvider IVideoValue.Provider => Provider;

        IVideoDetails IVideoValue.Details => Details;

        IVideoEmbed IVideoValue.Embed => Embed;

        #endregion

        #region Constructors

        private SkyfishVideoValue(JObject json, SkyfishVideoDetails details, SkyfishVideoEmbed embed) {
            Json = json;
            Type = "video";
            Source = json.GetString("source");
            Provider = SkyfishProvider.Default;
            Details = details;
            Embed = embed;
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Creates and returns a new <see cref="SkyfishVideoValue"/> instance based on the specified <paramref name="json"/> and <paramref name="config"/>.
        /// </summary>
        /// <param name="json">The JSOn object representing the video value.</param>
        /// <param name="config">The configuration of the TwentyThree data type.</param>
        /// <returns>An instance of <see cref="SkyfishVideoValue"/>.</returns>
        public static SkyfishVideoValue Create(JObject json, SkyfishConfiguration? config) {
            var details = json.GetObject("details", SkyfishVideoDetails.Parse)!;
            var embed = json.GetObject("embed", x => new SkyfishVideoEmbed(x, details, config))!;
            return new SkyfishVideoValue(json, details, embed);
        }

        #endregion

    }

}