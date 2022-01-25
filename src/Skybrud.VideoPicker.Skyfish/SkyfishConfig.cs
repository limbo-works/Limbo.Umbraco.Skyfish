using System.Xml.Linq;
using Skybrud.Essentials.Xml.Extensions;
using Skybrud.VideoPicker.Models.Config;

namespace Skybrud.VideoPicker.Skyfish {

    /// <summary>
    /// Class representing the configuration for the <strong>Skyfish</strong> provider.
    /// </summary>
    public class SkyfishConfig : IProviderConfig {

        #region Properties

        /// <summary>
        /// Gets an array with the configured credentials for this provider.
        /// </summary>
        public SkyfishCredentials[] Credentials { get; }

        #endregion

        #region Constructors

        internal SkyfishConfig(XElement xml) {
            Credentials = xml.GetElements("credentials", x => new SkyfishCredentials(x));
        }

        #endregion

    }

}