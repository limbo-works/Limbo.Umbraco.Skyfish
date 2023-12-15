using System;
using Limbo.Umbraco.Video.Models.Credentials;

namespace Limbo.Umbraco.Skyfish.Models.Settings {

    /// <summary>
    /// Class representing a single set of credentials used for accessing the Skyfish API.
    /// </summary>
    public class SkyfishCredentials : ICredentials {

        #region Properties

        /// <summary>
        /// Gets the key of the credentials.
        /// </summary>
        public Guid Key { get; internal set; }

        /// <summary>
        /// Gets the friendly name of the credentials.
        /// </summary>
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// Gets the description of the credentials.
        /// </summary>
        public string? Description { get; internal set; }

        /// <summary>
        /// Gets the public key of the credentials.
        /// </summary>
        public string PublicKey { get; internal set; } = null!;

        /// <summary>
        /// Gets the secret key of the credentials.
        /// </summary>
        public string SecretKey { get; internal set; } = null!;

        /// <summary>
        /// Gets the username of the credentials.
        /// </summary>
        public string Username { get; internal set; } = null!;

        /// <summary>
        /// Gets the password of the credentials.
        /// </summary>
        public string Password { get; internal set; } = null!;

        #endregion

    }

}