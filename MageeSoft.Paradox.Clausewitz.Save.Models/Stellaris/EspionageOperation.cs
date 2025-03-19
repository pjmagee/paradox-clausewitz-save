namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents an espionage operation in the game state.
/// </summary>
[SaveModel]
public partial class EspionageOperation
{
    /// <summary>
    /// Gets or sets the operation ID.
    /// </summary>
    public required long Id { get;set; }

    /// <summary>
    /// Gets or sets the type of operation.
    /// </summary>
    public required string Type { get;set; }

    /// <summary>
    /// Gets or sets the country performing the operation.
    /// </summary>
    public required int Country { get;set; }

    /// <summary>
    /// Gets or sets the target country of the operation.
    /// </summary>
    public required int TargetCountry { get;set; }

} 






