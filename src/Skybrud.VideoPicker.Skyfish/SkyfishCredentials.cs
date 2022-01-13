using System;
using System.Xml.Linq;
using Skybrud.Essentials.Xml.Extensions;
using Skybrud.VideoPicker.Models.Config;

namespace Skybrud.VideoPicker.Skyfish {
    
    /// <summary>
    /// Class representing a single set of credentials for 
    /// </summary>
    public class SkyfishCredentials : IProviderCredentials {
        
        #region Properties

        /// <summary>
        /// Gets the unique ID of the credentials.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Gets the friendly name of the credentials.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the public key of the credentials.
        /// </summary>
        public string PublicKey { get; }
        
        /// <summary>
        /// Gets the secret key of the credentials.
        /// </summary>
        public string SecretKey { get; }

        /// <summary>
        /// Gets the username of the credentials.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password of the credentials.
        /// </summary>
        public string Password { get; }
        
        /// <summary>
        /// Gets whether this credentials object is configured.
        /// </summary>
        public bool IsConfigured { get; }

        #endregion

        #region Constructors

        internal SkyfishCredentials(XElement xml) {
            Id = xml.GetAttributeValue("id", Guid.Parse);
            Name = xml.GetAttributeValue("name");
            PublicKey = xml.GetAttributeValue("apiKey");
            SecretKey = xml.GetAttributeValue("apiSecret");
            Username = xml.GetAttributeValue("username");
            Password = xml.GetAttributeValue("password");
            IsConfigured = !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey) && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        #endregion

    }

}