// [CHANGE: Umbraco 17 upgrade - registers a dedicated Swagger document for the package's Management API] Related: Api/SkyfishApiConstants.cs, Api/SkyfishSecurityFilter.cs, Composers/SkyfishComposer.cs

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Limbo.Umbraco.Skyfish.Api;

public class SkyfishSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions> {

    public void Configure(SwaggerGenOptions options) {

        options.SwaggerDoc(SkyfishApiConstants.Alias, new OpenApiInfo {
            Title = SkyfishApiConstants.Name,
            Version = "1.0"
        });

        options.OperationFilter<SkyfishSecurityFilter>();

    }

}
