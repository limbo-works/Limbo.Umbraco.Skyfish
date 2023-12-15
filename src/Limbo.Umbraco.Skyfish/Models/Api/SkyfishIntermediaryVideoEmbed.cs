using Newtonsoft.Json;

namespace Limbo.Umbraco.Skyfish.Models.Api;

internal class SkyfishIntermediaryVideoEmbed {

    [JsonProperty("url")]
    public string Url { get; }

    public SkyfishIntermediaryVideoEmbed(string url) {
        Url = url;
    }

}