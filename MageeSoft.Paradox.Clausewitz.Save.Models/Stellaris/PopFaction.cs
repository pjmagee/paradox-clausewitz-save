namespace MageeSoft.Paradox.Clausewitz.Save.Models.Stellaris;

/// <summary>
/// Represents a pop faction in the game state.
/// </summary>
public record PopFaction
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the support.
    /// </summary>
    public required float Support { get; init; }

    /// <summary>
    /// Gets or sets the approval.
    /// </summary>
    public required float Approval { get; init; }
}







