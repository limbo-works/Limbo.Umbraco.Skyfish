// [CHANGE: Umbraco 17 upgrade - labels/descriptions now live in the client side settings schema, and "hideLabel" was dropped] Related: wwwroot/EntryPoint.js, PropertyEditors/SkyfishVideoPropertyEditor.cs, PropertyEditors/SkyfishVideoValueConverter.cs

using Newtonsoft.Json;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

/// <summary>
/// Class representing the configuration of the <see cref="SkyfishVideoPropertyEditor"/> property editor.
/// </summary>
/// <remarks>
/// The label and description of each field are declared in the <c>settings</c> part of the property editor schema
/// registered from <c>EntryPoint.js</c>.
/// </remarks>
public class SkyfishVideoConfiguration {

    /// <summary>
    /// Gets or sets whether the JavaScript part of the default embed code should be removed.
    /// </summary>
    [ConfigurationField("removeJavaScript")]
    [JsonProperty("removeJavaScript")]
    public bool RemoveJavaScript { get; set; }

}
