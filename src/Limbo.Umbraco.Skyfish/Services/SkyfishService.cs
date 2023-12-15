using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Limbo.Integrations.Skyfish;
using Limbo.Integrations.Skyfish.Models.Media;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Options;
using Limbo.Umbraco.Video.Models.Videos;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.Strings;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Cache;

namespace Limbo.Umbraco.Skyfish.Services;

/// <summary>
/// Service for working with the Skyfish integration.
/// </summary>
public class SkyfishService {

    private readonly AppCaches _appCaches;
    private readonly IOptions<SkyfishSettings> _settings;

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified dependencies.
    /// </summary>
    public SkyfishService(AppCaches appCaches, IOptions<SkyfishSettings> settings) {
        _appCaches = appCaches;
        _settings = settings;
    }

    #endregion

    #region Member methods

    /// <summary>
    /// Returns whether the specified <paramref name="source"/> is recognized as a Skyfish URL.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="options">When this method returns, holds an instance of <see cref="SkyfishVideoOptions"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public bool TryParseSource(string source, [NotNullWhen(true)] out SkyfishVideoOptions? options) {

        options = null;
        if (string.IsNullOrWhiteSpace(source)) return false;

        if (RegexUtils.IsMatch(source, "^https://app.skyfish.com/folder/([0-9]+)/file/([0-9]+)$", out Match match)) {
            options = new SkyfishVideoOptions(null, match.Groups[2].Value.ToInt32());
            return true;
        }

        if (RegexUtils.IsMatch(source, "^https://www.skyfish.com/sh/([a-z0-9]+)/([0-9]+)/([0-9]+)/([0-9]+)$", out match)) {
            options = new SkyfishVideoOptions(match.Groups[4].Value.ToInt32(), null);
            return true;
        }

        return false;

    }

    /// <summary>
    /// Returns a list of Skyfish credentials.
    /// </summary>
    /// <returns>A collection of <see cref="SkyfishCredentials"/>.</returns>
    public IEnumerable<SkyfishCredentials> GetCredentials() {
        return _settings.Value.Credentials;
    }

    /// <summary>
    /// Creates a new HTTP service for accessing the Skyfish API using the specified <paramref name="credentials"/>.
    /// </summary>
    /// <param name="credentials">The credentials.</param>
    /// <param name="http">When this method returns, holds the created HTTP service if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
    public virtual bool TryGetHttpService(SkyfishCredentials credentials, [NotNullWhen(true)] out SkyfishHttpService? http) {

        if (credentials == null) throw new ArgumentNullException(nameof(credentials));

        http = null;

        if (string.IsNullOrWhiteSpace(credentials.PublicKey)) return false;
        if (string.IsNullOrWhiteSpace(credentials.SecretKey)) return false;
        if (string.IsNullOrWhiteSpace(credentials.Username)) return false;
        if (string.IsNullOrWhiteSpace(credentials.Password)) return false;

        string cacheKey = $"{SkyfishPackage.Alias}:{nameof(SkyfishHttpService)}:{credentials.Key}";

        http = (SkyfishHttpService) _appCaches.RuntimeCache.Get(cacheKey, () => CreateHttpService(credentials), TimeSpan.FromDays(14))!;

        return true;

    }

    private SkyfishHttpService CreateHttpService(SkyfishCredentials credentials) {
        return SkyfishHttpService.CreateFromKeys(credentials.PublicKey, credentials.SecretKey, credentials.Username, credentials.Password);
    }

    /// <summary>
    /// Returns a list of thumbnails for the specified <paramref name="video"/>.
    /// </summary>
    /// <param name="video">The video.</param>
    /// <returns>A list of <see cref="VideoThumbnail"/>.</returns>
    /// <remarks>
    /// Skyfish has short-lived thumbnail URLs, so this method will generate custom thumbnail URLs routed through
    /// Umbraco that will not expire since we can then handle the authentication "under the hood" in our custom
    /// endpoint.
    /// </remarks>
    public IReadOnlyList<VideoThumbnail> GetThumbnails(SkyfishMediaItem video) {
        List<VideoThumbnail> thumbnails = new() { GetThumbnail(video) };
        return thumbnails;
    }

    private VideoThumbnail GetThumbnail(SkyfishMediaItem video) {

        // Since the CDN thumbnail URL returned by the API expire over time, we need to "proxy" the thumbnail via an
        // endpoint that we control so that the URL we save in Umbraco doesn't actually expire
        string url = $"/api/skyfish/{video.UniqueMediaId}/thumbnail";

        // Return a new thumbnail. Notice that the Skyfish API doesn't explicitly tell us the size of the thumbnail,
        // but when testing with various videos across two different clients, the size seems to always be 320x180
        return new VideoThumbnail(320, 180, url);

    }

    #endregion

}