using Newtonsoft.Json;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.PropertyEditors;

public class SkyfishConfiguration {

    [ConfigurationField("hideLabel", "Hide label", "boolean", Description = "Select whether the label and description of properties using this data type should be hidden.<br /><br />Hiding the label and description can be useful in some cases - eg. to give the video picker a bit more horizontal space.")]
    [JsonProperty("hideLabel")]
    public bool HideLabel { get; set; }

    [ConfigurationField("removeJavaScript", "Remove JavaScript", "boolean", Description = "The default embed code contains a bit of JavaScript, which is not ideal in all cases. Enable this setting to remove the JavaScript from the embed code.")]
    [JsonProperty("removeJavaScript")]
    public bool RemoveJavaScript { get; set; }

}