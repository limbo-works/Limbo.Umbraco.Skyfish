// [CHANGE: Umbraco 17 upgrade - adds backoffice auth requirements to the package's Swagger document] Related: Api/SkyfishApiConstants.cs, Api/SkyfishSwaggerGenOptions.cs
// [CHANGE: code review fix - "ApiName" is matched against the value of the "MapToApi" attribute, so it must be the alias rather than the friendly name] Related: Controllers/SkyfishManagementController.cs, wwwroot/Elements/Video.js

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Umbraco.Cms.Api.Management.OpenApi;

namespace Limbo.Umbraco.Skyfish.Api;

public class SkyfishSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase {

    protected override string ApiName => SkyfishApiConstants.Alias;

}
