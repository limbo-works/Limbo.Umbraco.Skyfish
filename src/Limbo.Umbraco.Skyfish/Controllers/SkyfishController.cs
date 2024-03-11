using System.Linq;
using Limbo.Integrations.Skyfish;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

#pragma warning disable CS1591

namespace Limbo.Umbraco.Skyfish.Controllers;

public class SkyfishController : UmbracoApiController {

    private readonly SkyfishService _skyfishService;

    public SkyfishController(SkyfishService skyfishService) {
        _skyfishService = skyfishService;
    }

    [HttpGet]
    [Route("api/skyfish/{uniqueMediaId}/thumbnail")]
    [Route("umbraco/api/Skyfish/GetThumbnail")]
    public object GetThumbnail(string uniqueMediaId) {

        // Get the first credentials (or trigger an error if none)
        SkyfishCredentials? credentials = _skyfishService.GetCredentials().FirstOrDefault();
        if (credentials == null) return BadRequest("Skyfish provider is not configured (1).");
        if (!_skyfishService.TryGetHttpService(credentials, out SkyfishHttpService? http)) return BadRequest("Skyfish provider is not configured (2).");

        // Initialize a new service for the Skyfish API
        SkyfishHttpHelper skyHelper = new(http);

        string? thumbnailUrl = skyHelper.GetMediaByUniqueMediaId(int.Parse(uniqueMediaId))?.ThumbnailUrl;

        if (!string.IsNullOrWhiteSpace(thumbnailUrl)) return Redirect(thumbnailUrl);
        return NotFound();

    }

}