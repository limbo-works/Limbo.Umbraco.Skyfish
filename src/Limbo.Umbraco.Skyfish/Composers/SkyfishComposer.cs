// [CHANGE: Umbraco 17 upgrade - manifest filter replaced by manifest reader, added Swagger document registration] Related: Manifests/SkyfishPackageManifestReader.cs, Api/SkyfishSwaggerGenOptions.cs, Limbo.Umbraco.Skyfish.csproj

using Limbo.Umbraco.Skyfish.Api;
using Limbo.Umbraco.Skyfish.Extensions;
using Limbo.Umbraco.Skyfish.Manifests;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Manifest;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Limbo.Umbraco.Skyfish.Composers;

public class SkyfishComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {

        builder.Services.AddSingleton<SkyfishService>();

        builder.AddUmbracoOptions<SkyfishSettings>();

        builder.Services.AddSingleton<IPackageManifestReader, SkyfishPackageManifestReader>();

        builder.Services.ConfigureOptions<SkyfishSwaggerGenOptions>();

    }

}
