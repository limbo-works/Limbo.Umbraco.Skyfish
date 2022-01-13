using Skybrud.VideoPicker.Models.Options;

namespace Skybrud.VideoPicker.Skyfish {
    
    public class SkyfishVideoOptions : IVideoOptions {
        
        public string VideoId { get; }

        public SkyfishVideoOptions(string videoId) {
            VideoId = videoId;
        }

    }

}