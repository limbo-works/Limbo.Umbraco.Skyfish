namespace Limbo.Umbraco.Skyfish.Models.Api;

internal class SkyfishIntermediaryVideoValue {

    public SkyfishIntermediaryVideoDetails Details { get; }

    public SkyfishIntermediaryVideoEmbed Embed { get; }

    public SkyfishIntermediaryVideoValue(SkyfishIntermediaryVideoDetails details, SkyfishIntermediaryVideoEmbed embed) {
        Details = details;
        Embed = embed;
    }

    public bool ShouldSerializeEmbed() {
        return !string.IsNullOrWhiteSpace(Embed.Url);
    }

}