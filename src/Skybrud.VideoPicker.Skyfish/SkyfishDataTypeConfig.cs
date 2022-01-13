using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.VideoPicker.PropertyEditors.Video;

namespace Skybrud.VideoPicker.Skyfish {

    public class SkyfishDataTypeConfig : IProviderDataTypeConfig {

        [JsonProperty("enabled")]
        public bool IsEnabled { get; }

        [JsonProperty("consent")]
        public DataTypeConfigOption<bool> RequireConsent { get; }

        [JsonProperty("autoplay")]
        public DataTypeConfigOption<bool> Autoplay { get; }

        public SkyfishDataTypeConfig() {
            IsEnabled = false;
            RequireConsent = new DataTypeConfigOption<bool>(false);
            Autoplay = new DataTypeConfigOption<bool>(false);
        }

        public SkyfishDataTypeConfig(JObject value) {
            IsEnabled = value.GetBoolean("enabled");
            RequireConsent = new DataTypeConfigOption<bool>(value.GetBoolean("consent.value"));
            Autoplay = new DataTypeConfigOption<bool>(value.GetBoolean("autoplay.value"));
        }

    }

}