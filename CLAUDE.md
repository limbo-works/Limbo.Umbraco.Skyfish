# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

**Limbo.Umbraco.Skyfish** is a NuGet package (not an app) — an Umbraco backoffice property editor that lets editors paste a Skyfish URL/embed code and get a video value. There is no host Umbraco site and no test project in the repo; verification is done by building and installing the package into a consuming solution.

Current branch `v17/dev` targets **Umbraco 17 / .NET 10**. See `documentation/UPGRADE-UMBRACO-17.md` for what the migration from v13 changed and what has not been smoke tested yet.

## Commands

```
dotnet build src/Limbo.Umbraco.Skyfish.sln -c Release
dotnet pack src/Limbo.Umbraco.Skyfish/Limbo.Umbraco.Skyfish.csproj -c Release
```

Debug builds auto-append a `buildyyyyMMddHHmm` `VersionSuffix` (see csproj) so local packages always sort as prerelease. There is no client-side build step — the `wwwroot` JS is hand-written ESM shipped as-is via static web assets.

## Branch layout

One branch per Umbraco major: `v17/dev` (current), `v13/main`, `v10/main` (EOL). The sibling package **Limbo.Umbraco.TwentyThree** (`v17/dev` branch) is the other video picker built on the same `Limbo.Umbraco.Video` base and was the blueprint for the v17 migration — check it first when adding backoffice features, to keep the two consistent.

## Architecture

Flow when an editor pastes a source:

1. `wwwroot/Elements/Video.js` (`limbo-skyfish-video`, a Lit element) debounces input and calls
2. `wwwroot/Service.js` → `GET /umbraco/management/api/v1/limbo/skyfish/video?source=...`
3. `Controllers/SkyfishManagementController.cs` — catches each `Skyfish*Exception` and maps it to a `text/plain` message localized from `wwwroot/Lang/*.xml` (`limboSkyfish` area) →
4. `SkyfishService.GetIntermediaryVideoValue(source)` — the core: `TryParseSource` regex-matches app URL / share URL / iframe embed into `SkyfishVideoOptions` (`SkyfishSourceType`), then calls the Skyfish API (search → media tags for duration → `GetEmbedUrl`, which polls up to 120×1s while Skyfish transcodes) and returns a `SkyfishIntermediaryVideoValue`.
5. The JSON of that intermediary object **is** the property value stored in the database. The `Intermediary*` model shapes are therefore a persistence contract — changing a `[JsonProperty]` name breaks existing content.
6. On read, `SkyfishVideoValueConverter` → `SkyfishVideoValue.Create(json, config)` produces the published-content value implementing `IVideoValue` from the **Limbo.Umbraco.Video** package. Nothing calls the Skyfish API at render time.

Key consequences to preserve:

- **The video endpoint must return Newtonsoft JSON.** It uses `NewtonsoftJsonResult.Ok(...)`, not the Management API's default `System.Text.Json`, because the response JSON is what gets saved to the database (`duration` in particular is serialized as seconds by a Newtonsoft converter).
- **Thumbnails are proxied.** Skyfish CDN thumbnail URLs expire, so `SkyfishService.GetThumbnails` stores `/api/skyfish/{uniqueMediaId}/thumbnail` and the unauthenticated `SkyfishController` redirects to the live CDN URL at request time. Never store the raw CDN URL, and never change those routes — they are baked into saved values.
- **Credentials**: bound from `Limbo:Skyfish:Credentials` (array) via `SkyfishSettings`/`SkyfishCredentials`; only the *first* entry is ever used (`GetCredentials().FirstOrDefault()`), though the stored value records the credentials `Guid`. `SkyfishHttpService` instances are cached in `AppCaches.RuntimeCache` for 14 days keyed by credentials GUID.
- **Duration**: Umbraco/JSON.NET corrupts timestamps in nested objects, so `Video.js` stores the details as a serialized string (`details._data`) rather than a nested object, and `SkyfishVideoDetails.Parse` unpacks it.
- **Embed HTML** is built with HtmlAgilityPack in `SkyfishVideoEmbed.GetHtml`, including Skyfish's own inline `onLoad` resize script; the `removeJavaScript` data-type config (`SkyfishVideoConfiguration`) suppresses it.
- **The `Limbo.Umbraco.Skyfish` editor alias must not change** — it is stored on every data type using the editor.

### Registration

`SkyfishComposer` registers the service, the options, `SkyfishPackageManifestReader` and the Swagger document. The manifest reader (`Manifests/`) declares only the `backofficeEntryPoint` and the `@limbo/skyfish/*` import map; the localizations, icons, `propertyEditorSchema` and `propertyEditorUi` are all registered client side from `wwwroot/EntryPoint.js`, which needs the server-variable cache buster first. Package constants (alias, versions, URLs) live in `SkyfishPackage`.

When touching the property editor, remember both halves must agree: the C# `[DataEditor]` alias (`SkyfishVideoPropertyEditor.EditorAlias`) and the `propertyEditorSchemaAlias` in `EntryPoint.js`. Config field labels/descriptions live in the `settings` block in `EntryPoint.js`, not in `[ConfigurationField]` any more.

## Conventions

`src/.editorconfig` is authoritative and strict — read it before writing C#. Notably: CRLF, **no final newline**, 4 spaces, file-scoped namespaces, opening brace on the same line, `#region` grouping (`Properties` / `Constructors` / `Member methods` / `Static methods`), full XML docs on public members or a file-level `#pragma warning disable CS1591`, nullable enabled, Newtonsoft `[JsonProperty]` on all serialized members.

Keep the build at zero warnings — obsolete Umbraco/Skybrud APIs were all cleaned up during the v17 migration.

Bump `VersionPrefix` in the csproj and the `dotnet add package` version in `README.md` together when releasing.
