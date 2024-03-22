using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Limbo.Integrations.Skyfish;
using Limbo.Integrations.Skyfish.Models.Media;
using Limbo.Integrations.Skyfish.Options.Search;
using Limbo.Integrations.Skyfish.Responses.Media;
using Limbo.Integrations.Skyfish.Responses.Search;
using Limbo.Umbraco.Skyfish.Models.Api;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Options;
using Limbo.Umbraco.Skyfish.Services;
using Limbo.Umbraco.Video.Models.Videos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Strings;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

#pragma warning disable 1591

namespace Limbo.Umbraco.Skyfish.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SkyfishAuthorizedController : UmbracoAuthorizedApiController {

    private readonly ILogger<SkyfishAuthorizedController> _logger;
    private readonly GlobalSettings _globalSettings;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly SkyfishService _skyfishService;

    #region Constructors

    public SkyfishAuthorizedController(ILogger<SkyfishAuthorizedController> logger, IOptions<GlobalSettings> globalSettings, ILocalizedTextService localizedTextService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, SkyfishService skyfishService) {
        _logger = logger;
        _globalSettings = globalSettings.Value;
        _localizedTextService = localizedTextService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _skyfishService = skyfishService;
    }

    #endregion

    #region Public API methods

    [HttpGet]
    [HttpPost]
    [Route("umbraco/backoffice/Limbo/Skyfish/GetVideo")]
    public object GetVideo() {

        // Get the "source" parameter from either GET or POST
        string? source = HttpContext.Request.Query["source"];
        if (string.IsNullOrWhiteSpace(source) && HttpContext.Request.HasFormContentType) {
            source = HttpContext.Request.Form["source"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(source)) return NoSourceSpecified();

        // Try to get the Skyfish media ID from "source"
        if (!_skyfishService.TryParseSource(source, out SkyfishVideoOptions? options)) return InvalidSourceSpecified();

        // Get the first set of configured credentials (we don't currently support more than one)
        SkyfishCredentials? credentials = _skyfishService.GetCredentials().FirstOrDefault();
        if (credentials == null || !_skyfishService.TryGetHttpService(credentials, out SkyfishHttpService? http)) return NotConfigured();

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

            // Get the first media of the response (if asny)
            media = response.Body.Media.FirstOrDefault();
            if (media is null) return VideoNotFoundFromUrl();

            // TODO: Should we validate the media type to exclude non-video media types?

        } catch (Exception ex) {

            _logger.LogError(ex, "Failed getting media from the Skyfish API from specified source '{Source}'.", source);

            return GenericError();

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
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed getting media tags from the Skyfish API from specified source '{Source}'.", source);
            return GenericError();
        }

        // Get the embed URL of the video
        string? embedUrl = skyHelper.GetEmbedUrl(media.UniqueMediaId, 120, TimeSpan.FromSeconds(1));
        if (string.IsNullOrWhiteSpace(embedUrl)) return FailedGettingEmbedUrl();

        // As thumbnail URLs received from the Skyfish API expire over time, we need to create our own solution to handle thumbnails URLs
        IReadOnlyList<VideoThumbnail> thumbnails = _skyfishService.GetThumbnails(media);

        // Initialize the intermediary details for the video
        SkyfishIntermediaryVideoDetails details = new(media, duration, thumbnails);

        // Initialize the intermediary embed information for the video
        SkyfishIntermediaryVideoEmbed embed = new(embedUrl);

        JObject json = JObject.FromObject(new {
            credentials = new {
                key = credentials.Key
            },
            details,
            embed
        });

        return json;

    }

    private IActionResult GenericError() {
        if (!TryGetTranslation("errorGeneric", out string? message)) message = "An error occured on the server.";
        return BadRequest(message);
    }

    private IActionResult NoSourceSpecified() {
        if (!TryGetTranslation("errorNoSourceSpecified", out string? message)) message = "No source specified.";
        return NotFound(message);
    }

    private IActionResult InvalidSourceSpecified() {
        if (!TryGetTranslation("errorInvalidSourceSpecified", out string? message)) message = "Source doesn't match a valid URL.";
        return NotFound(message);
    }
    private IActionResult NotConfigured() {
        if (!TryGetTranslation("errorNotConfigured", out string? message)) message = "No credentials configured for Skyfish.";
        return NotFound(message);
    }

    private IActionResult VideoNotFoundFromUrl() {
        if (!TryGetTranslation("errorVideoNotFoundFromUrl", out string? message)) message = "A video with the specified URL could not be found.";
        return NotFound(message);
    }

    private IActionResult FailedGettingEmbedUrl() {
        if (!TryGetTranslation("errorFailedGettingEmbedUrl", out string? message)) message = "Failed determining the embed URL of the specified video.";
        return NotFound(message);
    }

    private bool TryGetTranslation(string alias, [NotNullWhen(true)] out string? result) {

        var culture = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?
            .GetUserCulture(_localizedTextService, _globalSettings) ?? CultureInfo.CurrentCulture;

        string temp = _localizedTextService.Localize("limboSkyfish", alias, culture);

        if (string.IsNullOrWhiteSpace(temp) || temp.StartsWith('[')) {
            result = null;
            return false;
        }

        result = temp;
        return true;

    }

    #endregion

}