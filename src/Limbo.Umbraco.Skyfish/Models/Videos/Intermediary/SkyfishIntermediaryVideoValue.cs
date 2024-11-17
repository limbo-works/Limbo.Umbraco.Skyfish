using System;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Newtonsoft.Json;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.Models.Videos.Intermediary;

public class SkyfishIntermediaryVideoValue {

    [JsonProperty("source")]
    public string Source { get; }

    [JsonProperty("credentials")]
    public Guid Credentials { get; }

    [JsonProperty("details")]
    public SkyfishIntermediaryVideoDetails Details { get; }

    [JsonProperty("embed")]
    public SkyfishIntermediaryVideoEmbed Embed { get; }

    public SkyfishIntermediaryVideoValue(string source, SkyfishCredentials credentials, SkyfishIntermediaryVideoDetails details, SkyfishIntermediaryVideoEmbed embed) {
        Source = source;
        Credentials = credentials.Key;
        Details = details;
        Embed = embed;
    }

    public bool ShouldSerializeEmbed() {
        return !string.IsNullOrWhiteSpace(Embed.Url);
    }

}