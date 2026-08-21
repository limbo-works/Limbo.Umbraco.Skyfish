// [CHANGE: Umbraco 17 upgrade - the backoffice endpoints are now Management API endpoints] Related: Controllers/SkyfishManagementController.cs, Api/SkyfishSwaggerGenOptions.cs, Api/SkyfishSecurityFilter.cs, Composers/SkyfishComposer.cs

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Limbo.Umbraco.Skyfish.Api;

public static class SkyfishApiConstants {

    public const string Route = "limbo/skyfish";

    public const string Alias = "limbo-skyfish-v1";

    public const string Name = "Limbo Skyfish API v1";

    public const string GroupName = "Limbo Skyfish";

}
