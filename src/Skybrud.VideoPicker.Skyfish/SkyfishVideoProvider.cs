using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Limbo.Integrations.Skyfish;
using Limbo.Integrations.Skyfish.Models;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.VideoPicker.Exceptions;
using Skybrud.VideoPicker.Models;
using Skybrud.VideoPicker.Models.Config;
using Skybrud.VideoPicker.Models.Options;
using Skybrud.VideoPicker.Models.Providers;
using Skybrud.VideoPicker.Models.Videos;
using Skybrud.VideoPicker.PropertyEditors.Video;
using Skybrud.VideoPicker.Providers;
using Skybrud.VideoPicker.Services;

namespace Skybrud.VideoPicker.Skyfish {

    public class SkyfishVideoProvider : IVideoProvider {

        public string Alias => "skyfish";

        public string Name => "Skyfish";

        public string ConfigView => "/App_Plugins/Skybrud.VideoPicker.Skyfish/Views/Config.html";

        public string EmbedView => "/App_Plugins/Skybrud.VideoPicker.Skyfish/Views/Config.html";//"/App_Plugins/Skybrud.VideoPicker/Views/DefaultProvider/Embed.html";

        public bool IsMatch(VideoPickerService service, string source, out IVideoOptions options) {

            options = null;

            if (!source.Contains("skyfish.com")) return false;

            var videoId = source.Split('/').LastOrDefault();

            // Return true as we have an video name at this point
            options = new SkyfishVideoOptions(videoId);
            return true;

        }

        public VideoPickerValue GetVideo(VideoPickerService service, IVideoOptions options) {

            if (!(options is SkyfishVideoOptions o)) return null;

            // Get a reference to the Skyfish provider configuration
            SkyfishConfig config = service.Config.GetConfig<SkyfishConfig>(this);

            // Get the first credentials (or trigger an error if none)
            SkyfishCredentials credentials = config?.Credentials.FirstOrDefault();
            if (credentials == null) throw new VideosException("Skyfish provider is not configured (1).");
            if (credentials.IsConfigured == false) throw new VideosException("Skyfish provider is not configured (2).");

            // Initialize a new service for the Skyfish API
            SkyfishHttpService api = SkyfishHttpService.CreateFromKeys(credentials.PublicKey, credentials.SecretKey, credentials.Username, credentials.Password);

            SkyfishVideo video = api.GetVideo(int.Parse(o.VideoId));

            // As thumbnail URLs received from the Skyfish API expire over time, we need to create our own solution to handle thumbnails URLs
            VideoThumbnail[] thumbnails = GetThumbnails(video, config, credentials);

            VideoProviderDetails provider = new VideoProviderDetails(Alias, Name);

            SkyfishVideoDetails details = new SkyfishVideoDetails(video, thumbnails);

            SkyfishEmbedOptions embed = new SkyfishEmbedOptions(details);

            return new VideoPickerValue(provider, new VideoProviderCredentialsDetails(credentials), details, embed);

        }

        public VideoPickerValue ParseValue(JObject obj, IProviderDataTypeConfig config) {

            VideoProviderDetails provider = new VideoProviderDetails(Alias, Name);

            VideoProviderCredentialsDetails credentials = obj.GetObject("credentials", VideoProviderCredentialsDetails.Parse);

            SkyfishVideoDetails details = obj.GetObject("details", SkyfishVideoDetails.Parse);

            SkyfishEmbedOptions embed = new SkyfishEmbedOptions(details, config as SkyfishDataTypeConfig);

            return new VideoPickerValue(provider, credentials, details, embed);
        }

        public IProviderConfig ParseConfig(XElement xml) {
            return new SkyfishConfig(xml);
        }

        public IProviderDataTypeConfig ParseDataTypeConfig(JObject obj) {
            return new SkyfishDataTypeConfig(obj);
        }

        internal bool IsValidName(string videoName) {
            return videoName != null && Regex.IsMatch(videoName, "^([0-9_]+)$");
        }

        internal VideoThumbnail[] GetThumbnails(SkyfishVideo video, SkyfishConfig config, SkyfishCredentials credentials) {

            List<VideoThumbnail> thumbnails = new List<VideoThumbnail>();

            if (video.ThumbnailUrl.HasValue()) thumbnails.Add(new VideoThumbnail(0, 0, video.ThumbnailUrl));
            if (video.ThumbnailUrlSsl.HasValue()) thumbnails.Add(new VideoThumbnail(0, 0, video.ThumbnailUrlSsl));

            return thumbnails.ToArray();

        }

    }

}