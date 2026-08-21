# Upgrade to Umbraco 17

Recap of the upgrade of **Limbo.Umbraco.Skyfish** from Umbraco 13 / .NET 8 to Umbraco 17 / .NET 10, done on the `v17/dev` branch.

The package version is `17.0.0-alpha000` and all Umbraco dependencies are pinned to `[17.0.0,17.999)`.

## Reference implementation

The sibling package [**Limbo.Umbraco.TwentyThree**](https://github.com/limbo-works/Limbo.Umbraco.TwentyThree) (branch `v17/dev`, released as `17.0.0-alpha001`) had already been migrated by the same author. Since it is the other video picker built on top of `Limbo.Umbraco.Video`, its structure was used as the blueprint for this upgrade so the two packages stay consistent — same folder layout, same manifest reader pattern, same `EntryPoint.js` / `Package.js` / `Service.js` / `Auth.js` split.

## Project file

| Before | After |
| --- | --- |
| `net8.0` | `net10.0` |
| `<LangVersion>12.0</LangVersion>` | removed (net10.0 defaults to C# 14) |
| `VersionPrefix` `13.0.2` | `17.0.0-alpha000` |
| `Limbo.Umbraco.Video` `13.0.0` | `17.0.0-alpha001` |
| `Umbraco.Cms.Core` / `Web.Website` `[13.0.0,13.999)` | `[17.0.0,17.999)` |
| `Umbraco.Cms.Web.BackOffice` | **removed** — the assembly no longer exists |
| — | added `Umbraco.Cms.Api.Management` `[17.0.0,17.999)` |
| — | added `Umbraco.Cms.Web.Common` `[17.0.0,17.999)` |
| — | added `Skybrud.Essentials` `1.1.68` and `Skybrud.Essentials.AspNetCore` `1.0.2` |
| `compilerconfig.json` items | **removed** — no more LESS compilation |

`Limbo.Integrations.Skyfish` stays on `1.0.0`; it targets `netstandard2.0` and is unaffected.

Also added `<Title>` and `<NuGetAuditMode>direct</NuGetAuditMode>`, and switched the documentation URL to `/docs/v17/`.

## Server side

### Package manifest: `IManifestFilter` → `IPackageManifestReader`

`Manifests/SkyfishManifestFilter.cs` was deleted and replaced by `Manifests/SkyfishPackageManifestReader.cs`. `IManifestFilter`, `PackageManifest.Scripts` and `PackageManifest.Stylesheets` no longer exist — the AngularJS script/stylesheet bundling model is gone.

The new manifest only declares:

- a single `backofficeEntryPoint` extension pointing at `EntryPoint.js`
- an `importmap` exposing the package's own modules as `@limbo/skyfish/auth`, `@limbo/skyfish/package` and `@limbo/skyfish/service`

Everything else (localizations, icons, the property editor schema and UI) is registered client side from `EntryPoint.js`, because that is where the cache buster from the server variables is available. The old reflection hack that set `PackageManifest.PackageId` on Umbraco 12+ is gone — `Id` is now a normal property.

### New Management API

`Controllers/SkyfishAuthorizedController.cs` was deleted: `UmbracoAuthorizedApiController` no longer exists. It is replaced by `Controllers/SkyfishManagementController.cs`, a `ManagementApiControllerBase` with:

- `[VersionedApiBackOfficeRoute("limbo/skyfish")]`, `[MapToApi("limbo-skyfish-v1")]`, `[ApiVersion("1.0")]`
- `[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]`

Endpoints moved from `/umbraco/backoffice/Limbo/Skyfish/GetVideo` to:

| Endpoint | Purpose |
| --- | --- |
| `GET /umbraco/management/api/v1/limbo/skyfish/serverVariables` | returns the package version and the cache buster used by `EntryPoint.js` |
| `GET /umbraco/management/api/v1/limbo/skyfish/video?source=...` | looks up a video — the old `GetVideo` endpoint |

`GetVideo` is now GET-only (the old endpoint accepted both GET and form POST); the client only ever used the source parameter.

Three supporting files were added so the endpoints get their own Swagger document, matching how the TwentyThree package does it:

- `Api/SkyfishApiConstants.cs` — route, alias, name and group name
- `Api/SkyfishSecurityFilter.cs` — `BackOfficeSecurityRequirementsOperationFilterBase`
- `Api/SkyfishSwaggerGenOptions.cs` — registers the document, wired up in `SkyfishComposer`

**Serialization:** the Management API serializes with `System.Text.Json`, which would have changed the JSON returned for the intermediary video value (in particular the `duration`, which is serialized as seconds by a Newtonsoft converter). Since that JSON *is* the property value saved to the database, the video endpoint returns `NewtonsoftJsonResult.Ok(...)` from `Skybrud.Essentials.AspNetCore` to keep the Newtonsoft contract byte-for-byte identical. This is why `Skybrud.Essentials.AspNetCore` was added.

**Errors:** user-facing errors are now returned as `text/plain` `ContentResult` rather than `BadRequest("string")` / `NotFound("string")`, so the property editor can display the message as-is. The localized messages themselves are unchanged and still come from `ILocalizedTextService` using the `limboSkyfish` area.

### Public thumbnail proxy

`Controllers/SkyfishController.cs` — the endpoint that proxies Skyfish's short-lived CDN thumbnail URLs — keeps both of its routes, because **the URLs are stored inside the property value** and must keep resolving for existing content:

- `/api/skyfish/{uniqueMediaId}/thumbnail`
- `/umbraco/api/Skyfish/GetThumbnail`

`UmbracoApiController` still exists in Umbraco 17 but is `[Obsolete]`, and its own obsolete message recommends regular ASP.NET Core API controllers. The controller was therefore changed to a plain `ControllerBase` with `[ApiController]`, plus `[ApiExplorerSettings(IgnoreApi = true)]` to keep it out of the OpenAPI documents. The `uniqueMediaId` parameter is now an `int` with an `:int` route constraint instead of a string that was `int.Parse`'d.

### Property editor

`SkyfishVideoPropertyEditor`:

- the `[DataEditor]` attribute is now just `[DataEditor(EditorAlias, ValueType = ValueTypes.Json)]` — `name`, `view`, `Group` and `Icon` are no longer part of the server-side registration, they moved to the client-side schema manifest
- `EditorView` was removed, and with it the `GetValueEditor` override that appended `?v={version}` to the view path for cache busting
- `IEditorConfigurationParser` is gone from the constructor
- `SupportsReadOnly = true` was set, and the UI declares `supportsReadOnly: true`
- added `EditorUiAlias` (`Limbo.Umbraco.Skyfish.Video.Ui`) as a constant

**The editor alias `Limbo.Umbraco.Skyfish` is unchanged.** It is stored on every data type using this editor, so changing it would break existing content.

`SkyfishVideoConfigurationEditor` lost the `IEditorConfigurationParser` constructor parameter.

`SkyfishVideoConfiguration`:

- `[ConfigurationField]` no longer takes name/description/view arguments — those now live in the `settings` block of the client-side schema manifest
- **`hideLabel` was removed.** It relied on the AngularJS `$scope.model.hideLabel` behaviour, which has no equivalent in the new backoffice, so keeping it would have shown a toggle that does nothing. Any `hideLabel` value stored on existing data types is simply ignored.
- `removeJavaScript` is unchanged and still controls whether the inline resize script is included in the embed code.

`SkyfishVideoValueConverter` — `propertyType.DataType.Configuration as SkyfishVideoConfiguration` became `propertyType.DataType.ConfigurationAs<SkyfishVideoConfiguration>()`.

### Models

`Limbo.Umbraco.Video` 17 changed `IVideoDetails.Thumbnails` and `IVideoDetails.Files` from `IEnumerable<T>` to `IReadOnlyList<T>`, so `SkyfishVideoDetails` was updated to match.

Obsolete Skybrud.Essentials APIs were also swapped for their current equivalents:

- `Skybrud.Essentials.Json.Extensions` → `Skybrud.Essentials.Json.Newtonsoft.Extensions`
- `TimeSpanSecondsConverter` → `TimeSpanConverter` (whose default format is `TimeSpanFormat.Seconds`, so the serialized output is unchanged)

The build is clean with **0 warnings and 0 errors**.

## Client side

The whole AngularJS layer was removed and rewritten as Lit web components.

**Deleted:**

| File | Reason |
| --- | --- |
| `wwwroot/Scripts/Controllers/Video.js` | AngularJS controller |
| `wwwroot/Scripts/Services/SkyfishService.js` | AngularJS service (`$http`, `$scope`) |
| `wwwroot/Views/Video.html` | AngularJS template |
| `wwwroot/Styles/Styles.less` / `.css` / `.min.css` | styles now live in the component's `static styles` |
| `wwwroot/BackOffice/Icons/*.svg` | icons are now JS modules |
| `compilerconfig.json` / `.defaults` | no LESS build step any more |

**Added:**

| File | Purpose |
| --- | --- |
| `wwwroot/EntryPoint.js` | `onInit` — grabs the auth token from `UMB_AUTH_CONTEXT`, fetches the server variables, then registers the localizations, icons, property editor schema and property editor UI |
| `wwwroot/Package.js` | holds the server variables (version, cache buster) |
| `wwwroot/Auth.js` | holds the OpenAPI token getter |
| `wwwroot/Service.js` | `fetch`-based client for the Management API endpoints, plus `getErrorMessage` |
| `wwwroot/Icons.js` + `wwwroot/Icons/Skyfish.js`, `SkyfishAlt.js` | the two SVGs as JS modules, registered as an `icons` extension (`limbo-skyfish`, `limbo-skyfish-alt`) |
| `wwwroot/Localization/en-US.js`, `da-DK.js` | backoffice localization for the `limboSkyfish` area |
| `wwwroot/Elements/Video.js` | the `limbo-skyfish-video` property editor UI element |

**Kept:** `wwwroot/Lang/en-US.xml` and `da-DK.xml`. These are still read by `ILocalizedTextService` for the *server-side* error messages returned by the Management API. The new `Localization/*.js` files cover the *client-side* strings. The TwentyThree package keeps both files for the same reason.

### The property editor UI element

`Elements/Video.js` reproduces the Umbraco 13 UI: a source field, refresh/clear buttons, an inline error message, and a details panel with thumbnail, ID, title, duration, description and a link to Skyfish Drive.

Notable points:

- extends `UmbFormControlMixin(UmbLitElement)`
- the field swaps between `uui-input` and `uui-textarea` depending on whether the pasted source contains `<` (an embed code), the same behaviour as the old controller. Because the element swaps, the form control is registered in `updated()` (add/remove) rather than once in `firstUpdated()`.
- lookups are debounced by 400 ms as the user types
- the duration is rendered with `<limbo-video-duration>`, imported as `@limbo/video/elements/duration` from the import map that `Limbo.Umbraco.Video` 17 publishes
- `angular.toJson`/`fromJson` were replaced with `JSON.stringify`/`JSON.parse`

**The stored value shape is unchanged**, which is what makes existing content keep working:

```json
{
  "source": "...",
  "credentials": "<guid>",
  "details": { "_data": "<serialized json>" },
  "embed": { "url": "..." }
}
```

The `details._data` indirection is still needed — Umbraco/JSON.NET corrupts timestamps in nested objects — so the element continues to serialize the details into a string. `SkyfishVideoDetails.Parse` reads it back out unchanged.

One behavioural change: clearing the field now sets the value to `undefined` instead of the empty string `""` that Umbraco 13 needed as a workaround for its null handling.

## Review

The `umbraco-review-checks` reference from the Umbraco backoffice skill was applied to the new element. Three issues were found and fixed:

- **UI-3 (non-UUI components)** — a native `<textarea>` became `uui-textarea`, and the native `<table>` in the details panel became a CSS-grid `<dl>`
- **UI-6 (accessibility)** — added a `label` to the source input, and `role="alert"` to the error message
- form control registration was moved from `firstUpdated()` to `updated()` so it survives the input/textarea swap

One deliberate deviation from the checks: **UI-1** asks for errors to be raised through the notification system. The lookup error is kept as an inline message next to the field instead, because it is feedback on the value the editor is currently typing (and it is what both Umbraco 13 and the TwentyThree package do). A transient toast on every debounced keystroke would be worse.

## Not verified

The solution builds clean and packs to `Limbo.Umbraco.Skyfish.17.0.0-alpha000.nupkg`, and all client-side modules parse. The package has **not** been run inside an actual Umbraco 17 instance, so the backoffice has not been smoke tested — that needs a host site plus a set of Skyfish API credentials. Worth checking first when one is available:

1. the property editor appears under the **Limbo** group when creating a data type
2. `EntryPoint.js` loads and the `limbo-skyfish-alt` icon renders
3. pasting an app URL, a share URL and an embed code each resolve to a video
4. an existing Umbraco 13 property value still renders in the editor and on the website
5. the thumbnail proxy route still redirects

## Also updated

- `README.md` — Umbraco 17 / .NET 10, new install version, `v13/main` added to the list of other versions
- `CLAUDE.md` — rewritten for the new architecture
