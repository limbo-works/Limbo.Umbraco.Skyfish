namespace Limbo.Umbraco.Skyfish.Exceptions;

/// <summary>
/// Exception class thrown when the Skyfish package isn't configured.
/// </summary>
public class SkyfishNotConfiguredException : SkyfishException {

    /// <summary>
    /// Initialize a new instance.
    /// </summary>
    public SkyfishNotConfiguredException() : base("No Skyfish credentials configured.") { }

}