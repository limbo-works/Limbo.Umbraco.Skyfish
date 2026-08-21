using System;
using Skybrud.Essentials.Reflection;
using Umbraco.Cms.Core.Semver;

namespace Limbo.Umbraco.Skyfish;

/// <summary>
/// Static class with various information and constants about the package.
/// </summary>
public class SkyfishPackage {

    /// <summary>
    /// Gets the alias of the package.
    /// </summary>
    public const string Alias = "Limbo.Umbraco.Skyfish";

    /// <summary>
    /// Gets the friendly name of the package.
    /// </summary>
    public const string Name = "Limbo Skyfish";

    /// <summary>
    /// Gets the version of the package.
    /// </summary>
    public static readonly Version Version = typeof(SkyfishPackage).Assembly.GetName().Version!;

    /// <summary>
    /// Gets the informational version of the package.
    /// </summary>
    public static readonly string InformationalVersion = ReflectionUtils
        .GetInformationalVersion(typeof(SkyfishPackage))
        .Split('+')[0];

    /// <summary>
    /// Gets the semantic version of the package.
    /// </summary>
    public static readonly SemVersion SemVersion = SemVersion.Parse(ReflectionUtils.GetInformationalVersion<SkyfishPackage>());

    /// <summary>
    /// Gets the URL of the GitHub repository for this package.
    /// </summary>
    public const string GitHubUrl = "https://github.com/limbo-works/Limbo.Umbraco.Skyfish";

    /// <summary>
    /// Gets the URL of the issue tracker for this package.
    /// </summary>
    public const string IssuesUrl = "https://github.com/limbo-works/Limbo.Umbraco.Skyfish/issues";

    /// <summary>
    /// Gets the URL of the documentation for this package.
    /// </summary>
    // [CHANGE: code review fix - the documentation URL was left pointing at the v13 docs during the Umbraco 17 upgrade] Related: Limbo.Umbraco.Skyfish.csproj, wwwroot/EntryPoint.js, Api/SkyfishSecurityFilter.cs
    public const string DocumentationUrl = "https://packages.limbo.works/limbo.umbraco.skyfish/docs/v17/";


}