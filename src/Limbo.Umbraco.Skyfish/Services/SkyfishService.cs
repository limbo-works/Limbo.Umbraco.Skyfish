using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Limbo.Integrations.Skyfish;
using Limbo.Integrations.Skyfish.Models.Media;
using Limbo.Integrations.Skyfish.Options.Search;
using Limbo.Integrations.Skyfish.Responses.Media;
using Limbo.Integrations.Skyfish.Responses.Search;
using Limbo.Umbraco.Skyfish.Exceptions;
using Limbo.Umbraco.Skyfish.Models;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Models.Videos.Intermediary;
using Limbo.Umbraco.Skyfish.Options;
using Limbo.Umbraco.Video.Models.Videos;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.Collections.Specialized.Extensions;
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
    public virtual bool TryParseSource(string source, [NotNullWhen(true)] out SkyfishVideoOptions? options) {

        options = null;
        if (string.IsNullOrWhiteSpace(source)) return false;

        source = source.Trim();

        if (RegexUtils.IsMatch(source, "^https://app.skyfish.com/folder/(root|[0-9]+)/file/([0-9]+)$", out Match match)) {
            options = new SkyfishVideoOptions(source, SkyfishSourceType.AppUrl, null, match.Groups[2].Value.ToInt32());
            return true;
        }

        if (RegexUtils.IsMatch(source, "^https://(www|share).skyfish.com/sh/([a-z0-9]+)/([a-z0-9]+)/([0-9]+)/([0-9]+)$", out match)) {
            options = new SkyfishVideoOptions(source, SkyfishSourceType.ShareUrl, match.Groups[5].Value.ToInt32(), null);
            return true;
        }

        if (TryParseIframe(source, out options)) return true;

        return false;

    }

    /// <summary>
    /// Attempts to parse the specified <paramref name="source"/> as an iframe embed code, and return an instance of <see cref="SkyfishVideoOptions"/> if successful.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="options">When this method returns, holds an instance of <see cref="SkyfishVideoOptions"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryParseIframe(string source, [NotNullWhen(true)] out SkyfishVideoOptions? options) {

        options = null;

        if (string.IsNullOrWhiteSpace(source)) return false;

        try {

            HtmlDocument document = new();
            document.LoadHtml(source);

            HtmlNode? iframe = (HtmlNode?) document.DocumentNode.SelectSingleNode("//iframe");
            if (iframe is null) return false;

            string src = iframe.GetAttributeValue("src", string.Empty);
            if (string.IsNullOrWhiteSpace(src)) return false;

            // HtmlAgilityPack may return an HTML-encoded URL, e.g. &amp;.
            src = HttpUtility.HtmlDecode(src);

            // Parse the query string from the iframe src URL. We split on '#' first to remove any fragment, then split on '?' to get the query string part, and finally parse it into a NameValueCollection.
            NameValueCollection query = HttpUtility.ParseQueryString(src.Split('#')[0].Split('?').Skip(1).FirstOrDefault() ?? string.Empty);

            // Try to get the "media" parameter from the query string and parse it as an integer. If it fails, return false.
            if (!query.TryGetInt32("media", out int uniqueMediaId)) return false;

            options = new SkyfishVideoOptions(source, SkyfishSourceType.Embed, null, uniqueMediaId);
            return true;

        } catch {

            return false;

        }

    }

    /// <summary>
    /// Returns a list of Skyfish credentials.
    /// </summary>
    /// <returns>A collection of <see cref="SkyfishCredentials"/>.</returns>
    public IEnumerable<SkyfishCredentials> GetCredentials() {
        return _settings.Value.Credentials;
    }

    /// <summary>
    /// Returns the credentials with the specified <paramref name="key"/>, or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="key">The GUID key of the credentials.</param>
    /// <returns>An instance of <see cref="SkyfishCredentials"/> if successful; otherwise, <see langword="null"/>.</returns>
    public SkyfishCredentials? GetCredentialsByKey(Guid key) {
        return _settings.Value.Credentials.FirstOrDefault(x => x.Key == key);
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
        List<VideoThumbnail> thumbnails = [GetThumbnail(video)];
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

    /// <summary>
    /// Attempts to look up the video identified by the specified <paramref name="source"/>, and return an instance of <see cref="SkyfishIntermediaryVideoValue"/> if successful. When serialize to JSON, the value equals the property value saved in the database for properties using the Skyfish video data type.
    /// </summary>
    /// <param name="source">The source (URL) as entered by the user.</param>
    /// <returns>An instance of <see cref="SkyfishIntermediaryVideoValue"/> if successful; otherwise, <see langword="null"/>.</returns>
    public virtual SkyfishIntermediaryVideoValue GetIntermediaryVideoValue(string source) {

        // Return null right away if no source
        if (string.IsNullOrWhiteSpace(source)) throw new SkyfishException("No source specified.");

        // Try to parse the source
        if (!TryParseSource(source, out SkyfishVideoOptions? options)) {
            throw new SkyfishInvalidSourceException(source);
        }

        // Get the video from the options
        return GetIntermediaryVideoValue(source, options);

    }

    /// <summary>
    /// Attempts to look up the video identified by the specified <paramref name="source"/>, and return an instance of <see cref="SkyfishIntermediaryVideoValue"/> if successful. When serialize to JSON, the value equals the property value saved in the database for properties using the Skyfish video data type.
    /// </summary>
    /// <param name="source">The source (URL) as entered by the user.</param>
    /// <param name="options">The video options.</param>
    /// <returns>An instance of <see cref="SkyfishIntermediaryVideoValue"/> representing the video.</returns>
    protected virtual SkyfishIntermediaryVideoValue GetIntermediaryVideoValue(string source, SkyfishVideoOptions options) {

        // Get the first set of configured credentials (we don't currently support more than one)
        SkyfishCredentials? credentials = GetCredentials().FirstOrDefault();
        if (credentials == null || !TryGetHttpService(credentials, out SkyfishHttpService? http)) throw new SkyfishNotConfiguredException();

        // Initialize a new helper instance
        SkyfishHttpHelper skyHelper = new(http);

        // Search for the video in the via the Search API
        SkyfishMediaItem? media;
        try {

            // Make the request to the API
            SkyfishSearchResponse response = http.Search.Search(new SkyfishSearchOptions {
                MediaId = options.MediaId ?? 0,
                UniqueMediaId = options.UniqueMediaId ?? 0
            });

            // Throw an exception if the API response doesn't contain any media
            if (response.Body.Media.Count == 0) throw new SkyfishVideoNotFoundException(source);

            // Get the first media of the response (if any)
            media = response.Body.Media[0];

            // TODO: Should we validate the media type to exclude non-video media types?

        } catch (Exception ex) when (ex is not SkyfishException) {
            throw new SkyfishException("Failed getting media from the Skyfish API from specified source.", ex) {
                Source = source
            };
        }

        // Get the duration of the video, if available
        TimeSpan? duration = null;
        try {
            SkyfishMediaTagsResponse response = http.Media.GetTags(media.UniqueMediaId);
            if (response.Body.QuickTime is not null && response.Body.QuickTime.TryGetValue("Duration", out object? durationValue)) {
                if (durationValue is string durationStr && StringUtils.TryParseDouble(durationStr, out double durationResult)) {
                    duration = TimeSpan.FromSeconds(durationResult);
                }
            }
        } catch (Exception ex) when (ex is not SkyfishException) {
            throw new SkyfishException("Failed getting media tags from the Skyfish API from specified source", ex) {
                Source = source
            };
        }

        // Get the embed URL of the video
        string? embedUrl;
        try {
            const int attempts = 120;
            TimeSpan interval = TimeSpan.FromSeconds(1);
            embedUrl = skyHelper.GetEmbedUrl(media.UniqueMediaId, attempts, interval);
            if (string.IsNullOrWhiteSpace(embedUrl)) throw new SkyfishEmbedUrlTimeoutException(source, media.UniqueMediaId, attempts, interval);
        } catch (Exception ex) when (ex is not SkyfishException) {
            throw new SkyfishException("Failed getting stream URL from the Skyfish API from specified source.", ex) {
                Source = source
            };
        }

        // As thumbnail URLs received from the Skyfish API expire over time, we need to create our own solution to handle thumbnails URLs
        IReadOnlyList<VideoThumbnail> thumbnails = GetThumbnails(media);

        // Initialize the intermediary details for the video
        SkyfishIntermediaryVideoDetails details = new(media, duration, thumbnails);

        // Initialize the intermediary embed information for the video
        SkyfishIntermediaryVideoEmbed embed = new(embedUrl);

        // Initialize a new intermediary video value
        return new SkyfishIntermediaryVideoValue(source, credentials, details, embed);

    }

    #endregion

}