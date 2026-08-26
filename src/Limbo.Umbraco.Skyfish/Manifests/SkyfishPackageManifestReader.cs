// [CHANGE: Umbraco 17 upgrade - IManifestFilter was replaced by IPackageManifestReader] Related: Composers/SkyfishComposer.cs, wwwroot/EntryPoint.js, wwwroot/Service.js

using System.Collections.Generic;
using System.Threading.Tasks;
using Skybrud.Essentials.Security.Extensions;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Limbo.Umbraco.Skyfish.Manifests;

/// <summary>
/// Package manifest reader for this package.
/// </summary>
/// <remarks>
/// The manifest only declares the backoffice entry point and the import map for the package's client side modules.
/// Everything else (localizations, the property editor schema and UI, icons) is registered from within
/// <c>EntryPoint.js</c> as that gives us access to the cache buster from the server variables.
/// </remarks>
public class SkyfishPackageManifestReader : IPackageManifestReader {

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync() {

        const string alias = SkyfishPackage.Alias;
        string cacheBuster = SkyfishPackage.InformationalVersion.ToMd5Hash();

        List<PackageManifest> manifests = [
            new() {
                Id = SkyfishPackage.Alias,
                Name = SkyfishPackage.Name,
                AllowTelemetry = true,
                Version = SkyfishPackage.InformationalVersion,
                Extensions = [
                    new {
                        name = $"{alias}.EntryPoint",
                        alias = $"{SkyfishPackage.Name}: Entry Point",
                        type = "backofficeEntryPoint",
                        js = $"/App_Plugins/{alias}/EntryPoint.js?v={cacheBuster}"
                    }
                ],
                Importmap = new PackageManifestImportmap {
                    Imports = new Dictionary<string, string> {
                        { "@limbo/skyfish/auth", $"/App_Plugins/{alias}/Auth.js?v={cacheBuster}" },
                        { "@limbo/skyfish/package", $"/App_Plugins/{alias}/Package.js?v={cacheBuster}" },
                        { "@limbo/skyfish/service", $"/App_Plugins/{alias}/Service.js?v={cacheBuster}" }
                    }
                }
            }
        ];

        return await Task.FromResult(manifests);

    }

}
