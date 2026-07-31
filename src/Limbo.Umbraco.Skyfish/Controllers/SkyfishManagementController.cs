// [CHANGE: Umbraco 17 upgrade - UmbracoAuthorizedApiController was removed, so this replaces SkyfishAuthorizedController] Related: Api/SkyfishApiConstants.cs, Composers/SkyfishComposer.cs, wwwroot/Service.js, wwwroot/Elements/Video.js

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Asp.Versioning;
using Limbo.Umbraco.Skyfish.Api;
using Limbo.Umbraco.Skyfish.Exceptions;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skybrud.Essentials.AspNetCore.Json.Newtonsoft;
using Skybrud.Essentials.Security.Extensions;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Limbo.Umbraco.Skyfish.Controllers;

/// <summary>
/// Management API controller used by the property editor in the backoffice.
/// </summary>
/// <remarks>
/// Endpoints are exposed under <c>/umbraco/management/api/v1/limbo/skyfish/</c> and require an authenticated
/// backoffice user. Looking up a video additionally requires access to the <strong>Content</strong> section, whereas
/// the server variables must be available to any backoffice user as the client side part of the package can't
/// register any of its extensions without them.
/// </remarks>
// [CHANGE: code review fix - the content section requirement was moved from the controller to the "video" endpoint, as
// "EntryPoint.js" can't register the property editor (or anything else) if the server variables are forbidden - eg. for
// a user who only has access to the settings section] Related: wwwroot/EntryPoint.js, wwwroot/Elements/Video.js
[ApiController]
[VersionedApiBackOfficeRoute(SkyfishApiConstants.Route)]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[MapToApi(SkyfishApiConstants.Alias)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = SkyfishApiConstants.GroupName)]
public class SkyfishManagementController : ManagementApiControllerBase {

    private readonly ILogger<SkyfishManagementController> _logger;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly SkyfishService _skyfishService;

    #region Constructors

    public SkyfishManagementController(ILogger<SkyfishManagementController> logger, IOptions<GlobalSettings> globalSettings, ILocalizedTextService localizedTextService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, SkyfishService skyfishService) {
        _logger = logger;
        _globalSettings = globalSettings;
        _localizedTextService = localizedTextService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _skyfishService = skyfishService;
    }

    #endregion

    #region Public API methods

    /// <summary>
    /// Returns the server variables needed by the client side part of the package.
    /// </summary>
    /// <returns>The server variables of this package.</returns>
    [HttpGet("serverVariables")]
    public object GetServerVariables() {
        return new {
            version = SkyfishPackage.InformationalVersion,
            cacheBuster = SkyfishPackage.InformationalVersion.ToMd5Hash()
        };
    }

    /// <summary>
    /// Returns information about the video matching the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source (URL or embed code) as entered by the user.</param>
    /// <returns>Information about the video matching <paramref name="source"/>.</returns>
    [HttpGet("video")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    public object GetVideo(string? source) {

        if (string.IsNullOrWhiteSpace(source)) return NoSourceSpecified();

        try {

            // The intermediary value is serialized using Newtonsoft.Json as the JSON it results in is what the
            // property editor saves to the database - and what the value converter later parses again
            return NewtonsoftJsonResult.Ok(_skyfishService.GetIntermediaryVideoValue(source));

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

    #endregion

    #region Private methods

    private IActionResult GenericError() {
        if (!TryGetTranslation("errorGeneric", out string? message)) message = "An error occured on the server.";
        return UserError(StatusCodes.Status500InternalServerError, message);
    }

    private IActionResult NoSourceSpecified() {
        if (!TryGetTranslation("errorNoSourceSpecified", out string? message)) message = "No source specified.";
        return UserError(StatusCodes.Status400BadRequest, message);
    }

    private IActionResult InvalidSourceSpecified() {
        if (!TryGetTranslation("errorInvalidSourceSpecified", out string? message)) message = "Source doesn't match a valid URL.";
        return UserError(StatusCodes.Status400BadRequest, message);
    }

    private IActionResult NotConfigured() {
        if (!TryGetTranslation("errorNotConfigured", out string? message)) message = "No credentials configured for Skyfish.";
        return UserError(StatusCodes.Status500InternalServerError, message);
    }

    private IActionResult VideoNotFoundFromUrl() {
        if (!TryGetTranslation("errorVideoNotFoundFromUrl", out string? message)) message = "A video with the specified URL could not be found.";
        return UserError(StatusCodes.Status404NotFound, message);
    }

    private IActionResult FailedGettingEmbedUrl() {
        if (!TryGetTranslation("errorFailedGettingEmbedUrl", out string? message)) message = "Failed determining the embed URL of the specified video.";
        return UserError(StatusCodes.Status500InternalServerError, message);
    }

    /// <summary>
    /// Returns a plain text error result, as the messages are shown as-is by the property editor.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="message">The message to be shown to the user.</param>
    /// <returns>An instance of <see cref="ContentResult"/>.</returns>
    private static IActionResult UserError(int statusCode, string message) {
        return new ContentResult {
            Content = message,
            ContentType = "text/plain",
            StatusCode = statusCode
        };
    }

    private bool TryGetTranslation(string alias, [NotNullWhen(true)] out string? result) {

        CultureInfo culture = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?
            .GetUserCulture(_localizedTextService, _globalSettings.Value) ?? CultureInfo.CurrentCulture;

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
