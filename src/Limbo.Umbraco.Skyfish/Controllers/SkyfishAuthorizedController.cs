using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Limbo.Umbraco.Skyfish.Exceptions;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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



        try {

            return _skyfishService.GetIntermediaryVideoValue(source);

        } catch (SkyfishInvalidSourceException ex) {

            _logger.LogError(ex, "Invalid source specified: {Source}", source);

            return InvalidSourceSpecified();

        } catch (SkyfishNotConfiguredException ex) {

            _logger.LogError(ex, "The Skyfish package has not been configured.");

            return NotConfigured();

        } catch (SkyfishVideoNotFoundException) {

            return VideoNotFoundFromUrl();

        } catch (SkyfishEmbedUrlTimeoutException ex) {

            _logger.LogError(ex, "Failed getting embed code for video: {Source}", source);

            return FailedGettingEmbedUrl();

        } catch (Exception ex) {

            _logger.LogError(ex, "Failed getting Skyfish video information: {Source}", source);

            return GenericError();

        }

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