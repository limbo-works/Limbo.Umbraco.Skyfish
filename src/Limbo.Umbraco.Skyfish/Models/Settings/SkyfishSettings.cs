using System.Collections.Generic;
using Umbraco.Cms.Core.Configuration.Models;

namespace Limbo.Umbraco.Skyfish.Models.Settings;

/// <summary>
/// Class representing the app settings for the <strong>Skyfish</strong> provider.
/// </summary>
[UmbracoOptions("Limbo:Skyfish", BindNonPublicProperties = true)]
public class SkyfishSettings {

    #region Properties

    /// <summary>
    /// Gets an array with the configured credentials for this provider.
    /// </summary>
    public List<SkyfishCredentials> Credentials { get; internal set; } = new();

    #endregion

}