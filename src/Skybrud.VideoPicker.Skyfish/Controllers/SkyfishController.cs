using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Limbo.Integrations.Skyfish;
using Skybrud.VideoPicker.Exceptions;
using Skybrud.VideoPicker.Services;
using Skybrud.WebApi.Json;
using Umbraco.Web.WebApi;

namespace Skybrud.VideoPicker.Skyfish.Controllers {
    [JsonOnlyConfiguration]
    public class SkyfishController : UmbracoApiController {

        private readonly VideoPickerService _videoPickerService;

        public SkyfishController(VideoPickerService videoPickerService) {
            _videoPickerService = videoPickerService;
        }

        [Route("umbraco/api/skyfish/GetThumbnail")]
        public object GetThumbnail(string videoId) {

            if (!_videoPickerService.Providers.TryGet(out SkyfishVideoProvider provider)) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!_videoPickerService.Config.TryGetConfig(provider, out SkyfishConfig config)) return Request.CreateResponse(HttpStatusCode.NotFound);

            // Get the first credentials (or trigger an error if none)
            SkyfishCredentials credentials = config.Credentials.FirstOrDefault();
            if (credentials == null) throw new VideosException("Skyfish provider is not configured (1).");
            if (credentials.IsConfigured == false) throw new VideosException("Skyfish provider is not configured (2).");

            // Initialize a new service for the Skyfish API
            SkyfishHttpService api = SkyfishHttpService.CreateFromKeys(credentials.PublicKey, credentials.SecretKey, credentials.Username, credentials.Password);

            var thumbnailUrl = api.GetThumbnailUrl(int.Parse(videoId));

            if (!string.IsNullOrWhiteSpace(thumbnailUrl)) return Redirect(thumbnailUrl);
            return Request.CreateResponse(HttpStatusCode.NotFound);

        }

    }

}
