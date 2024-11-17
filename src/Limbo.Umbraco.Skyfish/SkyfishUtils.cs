using Limbo.Umbraco.Skyfish.Models.Videos.Intermediary;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Limbo.Umbraco.Skyfish;

/// <summary>
/// Static class with various utility methods for Dream Broker implementation.
/// </summary>
public static class SkyfishUtils {

    /// <summary>
    /// Attempts to look up the video identified by the specified <paramref name="source"/>, and returns an instance of <see cref="SkyfishIntermediaryVideoValue"/> if successful. When serialized to JSON, the value equals the property value saved in the database for properties using the Skyfish video data type.
    /// </summary>
    /// <param name="source">The source (URL) as entered by the user.</param>
    /// <returns>An instance of <see cref="SkyfishIntermediaryVideoValue"/> representing the video.</returns>
    public static SkyfishIntermediaryVideoValue GetDreamBrokerVideoValue(string source) {
        return StaticServiceProvider.Instance
            .GetRequiredService<SkyfishService>()
            .GetIntermediaryVideoValue(source);
    }

}