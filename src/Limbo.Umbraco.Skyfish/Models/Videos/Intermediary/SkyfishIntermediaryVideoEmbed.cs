using Newtonsoft.Json;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.Models.Videos.Intermediary;

public class SkyfishIntermediaryVideoEmbed {

    [JsonProperty("url")]
    public string Url { get; }

    public SkyfishIntermediaryVideoEmbed(string url) {
        Url = url;
    }

}