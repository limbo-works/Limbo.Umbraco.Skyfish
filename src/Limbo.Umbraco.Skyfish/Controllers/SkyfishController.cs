// [CHANGE: Umbraco 17 upgrade - UmbracoApiController is obsolete, so this is now a regular ASP.NET Core API controller] Related: Controllers/SkyfishManagementController.cs, Services/SkyfishService.cs

using System.Linq;
using Limbo.Integrations.Skyfish;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.Controllers;

/// <summary>
/// Public controller used for proxying the short-lived thumbnail URLs of the Skyfish CDN.
/// </summary>
/// <remarks>
/// The URLs of this controller are saved as part of the property value, so the routes must not be changed. The
/// endpoint is deliberately left unauthenticated as the thumbnails are also used on the website.
/// </remarks>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class SkyfishController : ControllerBase {

    private readonly SkyfishService _skyfishService;

    public SkyfishController(SkyfishService skyfishService) {
        _skyfishService = skyfishService;
    }

    [HttpGet]
    [Route("api/skyfish/{uniqueMediaId:int}/thumbnail")]
    [Route("umbraco/api/Skyfish/GetThumbnail")]
    public IActionResult GetThumbnail(int uniqueMediaId) {

        // Get the first credentials (or trigger an error if none)
        SkyfishCredentials? credentials = _skyfishService.GetCredentials().FirstOrDefault();
        if (credentials == null) return BadRequest("Skyfish provider is not configured (1).");
        if (!_skyfishService.TryGetHttpService(credentials, out SkyfishHttpService? http)) return BadRequest("Skyfish provider is not configured (2).");

        // Initialize a new service for the Skyfish API
        SkyfishHttpHelper skyHelper = new(http);

        string? thumbnailUrl = skyHelper.GetMediaByUniqueMediaId(uniqueMediaId)?.ThumbnailUrl;

        if (!string.IsNullOrWhiteSpace(thumbnailUrl)) return Redirect(thumbnailUrl);
        return NotFound();

    }

}
