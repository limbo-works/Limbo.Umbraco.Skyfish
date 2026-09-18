using System.Collections.Generic;
using System.Threading.Tasks;
using Limbo.Umbraco.Skyfish.PropertyEditors;
using Skybrud.Essentials.Umbraco.Constants;
using Skybrud.Essentials.Umbraco.Manifests.Extensions;
using Skybrud.Essentials.Umbraco.Manifests.Extensions.EntryPoints;
using Skybrud.Essentials.Umbraco.Manifests.Extensions.Icons;
using Skybrud.Essentials.Umbraco.Manifests.Extensions.Localization;
using Skybrud.Essentials.Umbraco.Manifests.Extensions.PropertyEditors;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

using static Limbo.Umbraco.Skyfish.SkyfishPackage;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Limbo.Umbraco.Skyfish.Manifests;

/// <summary>
/// Package manifest reader for this package.
/// </summary>
public class SkyfishPackageManifestReader : IPackageManifestReader {

    public async Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync() {

        List<PackageManifest> manifests = [
            new() {
                Id = Alias,
                Name = Name,
                AllowTelemetry = true,
                Version = InformationalVersion,
                Extensions = [
                    ..GetExtensions(),
                    ..GetVideoExtensions()
                ],
                Importmap = new PackageManifestImportmap {
                    Imports = new Dictionary<string, string> {
                        { "@limbo/skyfish/auth", $"/App_Plugins/{Alias}/Auth.js" },
                        { "@limbo/skyfish/package", $"/App_Plugins/{Alias}/Package.js" },
                        { "@limbo/skyfish/service", $"/App_Plugins/{Alias}/Service.js" }
                    }
                }
            }
        ];

        return await Task.FromResult(manifests);

    }

    public static IEnumerable<IExtension> GetExtensions() {

        yield return new BackofficeEntryPointExtension {
            Alias = $"{Alias}.EntryPoint",
            Name = $"{Name}: Entry Point",
            Js = $"/App_Plugins/{Alias}/EntryPoint.js"
        };

        yield return new LocalizationExtension {
            Alias = $"{Alias}.Localization.EnUs",
            Name = $"{Name}: English (en-US)",
            Js = $"/App_Plugins/{Alias}/Localization/en-US.js",
            Meta = new LocalizationMeta {
                Culture = "en"
            }
        };

        yield return new LocalizationExtension {
            Alias = $"{Alias}.Localization.DaDk",
            Name = $"{Name}: Danish (da-DK)",
            Js = $"/App_Plugins/{Alias}/Localization/da-DK.js",
            Meta = new LocalizationMeta {
                Culture = "da"
            }
        };

        yield return new IconsExtension {
            Alias = $"{Alias}.Icons",
            Name = $"{Name}: Icons",
            Js = $"/App_Plugins/{Alias}/Icons.js"
        };

    }

    private static IEnumerable<IExtension> GetVideoExtensions() {

        yield return new PropertyEditorSchemaExtension {
            Alias = SkyfishVideoPropertyEditor.EditorAlias,
            Name = $"{Name}: Video Property Editor Schema",
            Meta = new PropertyEditorSchemaMeta {
                DefaultPropertyEditorUiAlias = SkyfishVideoPropertyEditor.EditorUiAlias,
                Settings = new PropertyEditorSettings {
                    Properties = [
                        new PropertyEditorSettingsProperty {
                            Alias = "removeJavaScript",
                            Label = "Remove JavaScript",
                            Description = "The default embed code contains a bit of JavaScript, which is not ideal in all cases. Enable this setting to remove the JavaScript from the embed code.",
                            PropertyEditorUiAlias = UmbracoPropertyEditorUiAliases.Toggle
                        }
                    ],
                    DefaultData = [
                        new PropertyEditorSettingsDefaultData { Alias = "removeJavaScript", Value = false }
                    ]
                }
            }
        };

        yield return new PropertyEditorUiExtension {
            Alias = SkyfishVideoPropertyEditor.EditorUiAlias,
            Name = $"{Name}: Video Property Editor UI",
            Js = $"/App_Plugins/{Alias}/Elements/Video.js",
            ElementName = "limbo-skyfish-video",
            Meta = new PropertyEditorUiMeta {
                Label = SkyfishVideoPropertyEditor.EditorName,
                PropertyEditorSchemaAlias = SkyfishVideoPropertyEditor.EditorAlias,
                Icon = SkyfishVideoPropertyEditor.EditorIcon,
                Group = SkyfishVideoPropertyEditor.EditorGroup,
                SupportsReadOnly = true
            }
        };

    }

}