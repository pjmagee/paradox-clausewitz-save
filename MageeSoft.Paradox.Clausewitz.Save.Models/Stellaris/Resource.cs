namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a resource in the game state.
/// </summary>
public record Resource
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public required float Amount { get; init; }

} 






