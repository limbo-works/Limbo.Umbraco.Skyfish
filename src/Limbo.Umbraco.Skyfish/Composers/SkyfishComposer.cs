using Limbo.Umbraco.Skyfish.Extensions;
using Limbo.Umbraco.Skyfish.Manifests;
using Limbo.Umbraco.Skyfish.Models.Settings;
using Limbo.Umbraco.Skyfish.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.Skyfish.Composers;

internal class SkyfishComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {

        builder.Services.AddSingleton<SkyfishService>();

        builder.ManifestFilters().Append<SkyfishManifestFilter>();

        builder.AddUmbracoOptions<SkyfishSettings>();

    }

}