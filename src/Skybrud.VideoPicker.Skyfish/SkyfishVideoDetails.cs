using Limbo.Integrations.Skyfish.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.VideoPicker.Models.Videos;

namespace Skybrud.VideoPicker.Skyfish
{

    public class SkyfishVideoDetails : IVideoDetails {

        #region Properties

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("description")]
        public string Description { get; }

        [JsonProperty("embedUrl")]
        public string EmbedUrl { get; }

        [JsonProperty("thumbnails")]
        public VideoThumbnail[] Thumbnails { get; }

        #endregion

        #region Constructors

        public SkyfishVideoDetails(SkyfishVideo video, VideoThumbnail[] thumbnails) {
            Id = video.VideoId.ToString();
            Title = string.IsNullOrWhiteSpace(video.VideoTitle) ? video.FileName : video.VideoTitle;
            Description = video.VideoDescription;
            EmbedUrl = video.EmbedUrl;
            Thumbnails = thumbnails;
        }

        private SkyfishVideoDetails(JObject obj) {
            Id = obj.GetString("id");
            Title = obj.GetString("title");
            Description = obj.GetString("description");
            EmbedUrl = obj.GetString("embedUrl");
            Thumbnails = obj.GetArrayItems("thumbnails", VideoThumbnail.Parse);
        }

        public static SkyfishVideoDetails Parse(JObject obj) {
            return obj == null ? null : new SkyfishVideoDetails(obj);
        }

        #endregion

    }

}